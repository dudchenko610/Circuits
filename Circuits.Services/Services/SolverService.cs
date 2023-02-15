using Accord.Math;
using Circuits.Services.Extensions;
using Circuits.Shared.Converters;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Solver;
using Circuits.ViewModels.Helpers;
using Newtonsoft.Json;

namespace Circuits.Services.Services;

public class SolverService
{
    public event EventHandler<string>? FeedbackEventCallback;
    public event EventHandler<string>? CompletionEventCallback;

    public async Task RunAsync(string equationSystemSerialized, int iterationCount = 100, float dx = 0.01f,
        float dt = 0.001f, float epsilon = 0.000001f)
    {
        /* 1. deserialize equation system */
        var equationSystem = JsonConvert.DeserializeObject<EquationSystem>(equationSystemSerialized, 
                new TypeConverter<Expression>());
        if (equationSystem is null) return;
        // After serialization variable references are loosen, we need to recover them
        SubstituteVariables(equationSystem); 

        var variables = equationSystem.Variables.ToList();
        var matrix = equationSystem.Matrix;

        /* 2. build calculation equations */
        var functions = GetFunctions(equationSystem);
        foreach (var function in functions) Console.WriteLine(function.GetLabel());

        /////////////////////////////////////

        // var functions = new List<Expression>();
        // for (var v = 0; v < variables.Count; v++)
        // {
        //     var expression = equationSystem.Matrix[v][variables.Count]; // right side
        //
        //     for (var j = v + 1; j < variables.Count; j++)
        //     {
        //         var variable = variables[j];
        //         expression = ExpressionHelper.Subtract(
        //             expression,
        //             ExpressionHelper.Multiply(variable, equationSystem.Matrix[v][j])
        //         );
        //     }
        //
        //     expression = ExpressionHelper.Divide(expression, equationSystem.Matrix[v][v]);
        //     functions.Add(expression);
        // }

        var solverVariableInfos = variables
            .Select(x => new SolverVariableInfo())
            .ToList();

        // calculation
        for (var i = 0; i < iterationCount; i++)
        {
            /*
            for (var j = variables.Count - 1; j >= 0 ; j--)
            {
                var func = functions[j];
                var variable = variables[j];
            
                variable.Value = func.Value; // calculation
                solverVariableInfos[j].Array.Add((float) variable.Value);
            }*/

            // Broyden's method below
            {
                var xK0 = new double[variables.Count].Transpose(); // set zeroes
                var jK0 = new double[matrix.Length, matrix.Length];
                CalculateJacobian(jK0, variables, functions, xK0, dx); // just recompute

                var jK0Inv = jK0.Inverse();
                var fK0 = new double[functions.Count].Transpose();
                CalculateFunction(fK0, functions, variables, xK0);

                var n = 0;

                for (var j = 0; j < 20; j++)
                {
                    var xK1 = xK0.Subtract(jK0Inv.Dot(fK0));
                    var b = xK1.Subtract(xK0);

                    var fK1 = new double[functions.Count].Transpose();
                    CalculateFunction(fK1, functions, variables, xK1);

                    var u = fK1;

                    var bTransposed = b.Transpose();
                    var bTb = bTransposed.Dot(b);

                    var v = b.Multiply(1.0f / bTb[0, 0]);
                    var vT = v.Transpose();

                    var numerator = jK0Inv.Dot(u).Dot(vT).Dot(jK0Inv);
                    var denominator = 1 + vT.Dot(jK0Inv).Dot(u)[0, 0];

                    var addition = numerator.Multiply(1.0f / denominator);

                    var jK1Inv = jK0Inv.Subtract(addition);

                    // Console.WriteLine("jK0Inv");
                    // Console.WriteLine(jK0Inv.ToCSharp());
                    //
                    // Console.WriteLine("jK1Inv");
                    // Console.WriteLine(jK1Inv.ToCSharp());
                    //
                    // Console.WriteLine("b");
                    // Console.WriteLine(b.ToCSharp());
                    //
                    // Console.WriteLine("x");
                    // Console.WriteLine(xK1.ToCSharp());
                    //
                    // Console.WriteLine($"____________________{j}____________________");
                    // Console.WriteLine($"_____________________{j}_____________________");
                    // Console.WriteLine($"____________________{j}____________________");

                    jK0Inv = jK1Inv;
                    fK0 = fK1;
                    xK0 = xK1;

                    n = j;
                    
                    var bMax = 0.0;
                    
                    foreach(var e in b)
                    {
                        var absE = Math.Abs(e);
                        if (absE > bMax) bMax = absE;
                    }
                    
                    if (bMax <= epsilon) break; // epsilon constraint
                } // max iterations constraint

                Console.WriteLine($"BROYDEN'S ITERATION COUNT: {n}");
                
                for (var j = 0; j < variables.Count; j++)
                {
                    var variable = variables[j];
                    variable.Value = xK0[j, 0];
                    
                    solverVariableInfos[j].Array.Add((float) variable.Value);
                }
            }

            for (var j = 0; j < variables.Count; j++)
            {
                var variable = variables[j];
                if (variable is not ExpressionDerivative derivative) continue;

                var integralVariable = derivative.Variable;

                solverVariableInfos[j].IntegralArray.Add((float)integralVariable.Value);
                integralVariable.Value += derivative.Value * dt;
            }

            // send each 10 value
            if ((i + 1) % 10 == 0)
            {
                var feedbackData = solverVariableInfos
                    .Select(x => new SolverVariableInfo()
                    {
                        Array = x.Array.Skip(Math.Max(0, x.Array.Count - 10)).ToList(),
                        IntegralArray = x.IntegralArray.Skip(Math.Max(0, x.IntegralArray.Count - 10)).ToList(),
                    })
                    .ToList();

                var serializedData = JsonConvert.SerializeObject(feedbackData);

                FeedbackEventCallback?.Invoke(this, serializedData);
            }

            await Task.Delay(5);
        }

        CompletionEventCallback?.Invoke(this, "completed");
    }

    private static void CalculateFunction(double[,] f, IList<Expression> functions,
        IList<ExpressionVariable> variables, double[,] x)
    {
        // 1. Map x to variables
        for (var i = 0; i < variables.Count; i++)
        {
            variables[i].Value = x[i, 0];
        }

        // 2. Calculate function values
        for (var i = 0; i < functions.Count; i++)
        {
            f[i, 0] = functions[i].Value;
        }
    }

    private static void CalculateJacobian(double[,] jacobian, IList<ExpressionVariable> variables,
        IList<Expression> functions, double[,] x, double dx)
    {
        for (var i = 0; i < functions.Count; i++)
        {
            // iterate over rows

            for (var j = 0; j < functions.Count; j++)
            {
                // reset variables
                for (var k = 0; k < variables.Count; k++)
                {
                    var variable = variables[k];
                    variable.Value = x[k, 0];
                }

                variables[j].Value += dx;
                jacobian[i, j] = functions[i].Value / dx; // partial derivative
            }
        }
    }

    private static void SubstituteVariables(EquationSystem equationSystem)
    {
        var variables = equationSystem.Variables.ToList();

        foreach (var variable in equationSystem.Variables)
        {
            if (variable is not ExpressionDerivative firstDerivative || firstDerivative == null!) continue;
            if (firstDerivative.Variable == null) continue;

            variables.Add(firstDerivative.Variable);

            if (firstDerivative.Variable is ExpressionDerivative secondDerivative)
                variables.Add(secondDerivative.Variable);
        }
        
        // seems like variables are not substituted completely, or maybe not because we start with derivatives

        foreach (var row in equationSystem.Matrix)
        {
            for (var j = 0; j < row.Length; j++)
            {
                var expression = row[j];
                SubstituteVariables(expression, variables);
                if (expression is not ExpressionVariable or ExpressionDerivative) continue;

                var variable = variables.FirstOrDefault(x => x.GetLabel() == expression.GetLabel());
                if (variable == null) continue;

                row[j] = variable;
            }
        }
    }

    private static void SubstituteVariables(Expression expression, IList<ExpressionVariable> variables)
    {
        switch (expression)
        {
            case ExpressionAdditions additions:
            {
                for (var i = 0; i < additions.Nodes.Count; i++)
                {
                    var addition = additions.Nodes[i];
                    if (addition is not ExpressionVariable or ExpressionDerivative)
                        SubstituteVariables(addition, variables);

                    var variable = variables.FirstOrDefault(x => x.GetLabel() == addition.GetLabel());
                    if (variable == null) continue;

                    additions.Nodes.Remove(addition);
                    additions.Nodes.Insert(i, variable);
                }

                break;
            }
            case ExpressionMultipliers multipliers:
            {
                for (var i = 0; i < multipliers.Nodes.Count; i++)
                {
                    var multiplier = multipliers.Nodes[i];
                    if (multiplier is not ExpressionVariable or ExpressionDerivative)
                        SubstituteVariables(multiplier, variables);

                    var variable = variables.FirstOrDefault(x => x.GetLabel() == multiplier.GetLabel());
                    if (variable == null) continue;

                    multipliers.Nodes.Remove(multiplier);
                    multipliers.Nodes.Insert(i, variable);
                }

                break;
            }
            case ShockleyDiodeEquation shockleyDiode:
            {
                var variable = variables.FirstOrDefault(x => x.GetLabel() == shockleyDiode.Variable.GetLabel());

                if (variable == null) throw new Exception("Variable for ShockleyDiodeEquation is not found!");

                shockleyDiode.Variable = variable;
                
                break;
            }
        }
    }

    private static List<Expression> GetFunctions(EquationSystem equationSystem)
    {
        var list = new List<Expression>();

        foreach (var t in equationSystem.Matrix)
        {
            // var vr = systemVars[v];
            Expression expression = new ExpressionValue(0.0f); // first element in row

            for (var j = 0; j < equationSystem.Variables.Count; j++)
            {
                var cellValue = t[j];
                if (cellValue is ExpressionValue { Value: 0.0f }) continue;

                var variable = equationSystem.Variables[j];

                expression = ExpressionHelper.Add(
                    expression,
                    ExpressionHelper.Multiply(variable, cellValue)
                );
            }

            expression = ExpressionHelper.Subtract(expression, t[equationSystem.Variables.Count]); // right side

            list.Add(expression);
        }

        return list;
    }
}