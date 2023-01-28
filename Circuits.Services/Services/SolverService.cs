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

    public async Task RunAsync(string equationSystemSerialized, int iterationCount = 100, float dx = 0.001f, float dt = 0.001f)
    {
        /* 1. deserialize equation system */
        var equationSystem =
            JsonConvert.DeserializeObject<EquationSystem>(equationSystemSerialized, new TypeConverter<Expression>());
        if (equationSystem is null) return;
        SubstituteVariables(equationSystem); // After serialization variable references are loosen, we need to recover them

        var variables = equationSystem.Variables.ToList();
        var matrix = equationSystem.Matrix;
        
        // /* 2. build calculation equations */
        // var functions = GetFunctions(equationSystem);
        // var x = Enumerable.Repeat(0.0f, variables.Count).ToList();
        //
        // foreach (var function in functions)
        // {
        //     Console.WriteLine(function.GetLabel());
        // }
        //
        // var jacobian = new float[matrix.Length, matrix.Length];
        // CalculateJacobian(jacobian, variables, functions, x, dx);
        //
        // Console.WriteLine("Jacobian");
        // Console.WriteLine(jacobian.ToCSharp());
        //
        // var jacobianInverse = jacobian.Inverse();
        // Console.WriteLine("Jacobian Inverse");
        // Console.WriteLine(jacobianInverse.ToCSharp());
        //
        // var multiplication = jacobian.Dot(jacobianInverse);
        //
        // Console.WriteLine("Multiplication");
        // Console.WriteLine(multiplication.ToCSharp());
        
        // try
        // {
        // }
        // catch (Exception e)
        // {
        //     Console.WriteLine(e.Message);
        //     Console.WriteLine(e.StackTrace);
        // }

        var functions = new List<Expression>();
        var solverVariableInfos = variables
            .Select(x => new SolverVariableInfo())
            .ToList();

        for (var v = 0; v < variables.Count; v++)
        {
            var expression = equationSystem.Matrix[v][variables.Count]; // right side

            for (var j = v + 1; j < variables.Count; j++)
            {
                var variable = variables[j];
                expression = ExpressionHelper.Subtract(
                    expression,
                    ExpressionHelper.Multiply(variable, equationSystem.Matrix[v][j])
                );
            }

            expression = ExpressionHelper.Divide(expression, equationSystem.Matrix[v][v]);
            functions.Add(expression);
        }

        // calculation
        for (var i = 0; i < iterationCount; i++)
        {
            for (var j = variables.Count - 1; j >= 0 ; j--)
            {
                var func = functions[j];
                var variable = variables[j];

                variable.Value = func.Value; // calculation
                solverVariableInfos[j].Array.Add((float) variable.Value);
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
                //
                // foreach (var item in feedbackData)
                // {
                //     Console.WriteLine(item.Array.ToArray().ToCSharp());
                //     Console.WriteLine(item.IntegralArray.ToArray().ToCSharp());
                //     Console.WriteLine("________________________________________");
                // }

                var serializedData = JsonConvert.SerializeObject(feedbackData);

                FeedbackEventCallback?.Invoke(this, serializedData);
            }

            await Task.Delay(5);
        }
        
        CompletionEventCallback?.Invoke(this, "completed");
    }

    private static void CalculateJacobian(float[,] jacobian, IList<ExpressionVariable> variables,
        IList<Expression> functions, IList<float> x, float dx)
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
                    variable.Value = x[k];
                }

                variables[j].Value += dx;
                jacobian[i, j] = (float)functions[i].Value / dx; // partial derivative
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

        foreach (var row in equationSystem.Matrix)
        {
            for (var j = 0; j < row.Length; j++)
            {
                var expression = row[j];
                if (expression is not ExpressionVariable or ExpressionDerivative)
                    SubstituteVariables(expression, variables);

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