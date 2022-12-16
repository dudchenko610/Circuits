using Circuits.Services.Extensions;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Helpers;

namespace Circuits.Services.Services;

public class JSEquationSystemSolver : IJSEquationSystemSolver
{
    public Action<List<double>>? TestReadyCallback { get; set; }
    
    private readonly ISchemeService _schemeService;
    
    public JSEquationSystemSolver(ISchemeService schemeService)
    {
        _schemeService = schemeService;
    }

    public void BuildJsFunctions()
    {
        foreach (var equationSystem in _schemeService.EquationSystems)
        {
            var index = 0;

            for (var i = equationSystem.Matrix.Length; i > -1; i--)
            {
                var function = $"function getValueFor() {{";
                
                function += "}";
                index++;
            }
        }
    }

    public async Task TestSolveAsync()
    {
        var equationSystem = _schemeService.EquationSystems.FirstOrDefault();
        if (equationSystem == null!) return;

        var systemVars = equationSystem.Variables;

        // setup arrays
        var arraysHolder = new Dictionary<ExpressionVariable, List<double>>();
        
        _schemeService.SolverResult.Clear();
        _schemeService.SolverResult.Add(equationSystem, arraysHolder);

        /* 1. set initial values for variables with derivatives */
        foreach (var variable in systemVars)
        {
            var array = new List<double>();
            arraysHolder.Add(variable, array);
            //
            // if (variable is ExpressionDerivative) array.Add(0);
        }

        var calculationEquations = new List<Expression>();
        
        /* 2. build calculation equations */
        for (var v = systemVars.Count - 1; v > -1; v--)
        {
            var vr = systemVars[v];
            var expression = equationSystem.Matrix[v][systemVars.Count]; // right side

            for (var j = v + 1; j < systemVars.Count; j ++)
            {
                var variable = systemVars[j];
                expression = ExpressionHelper.Subtract(
                    expression, 
                    ExpressionHelper.Multiply(variable, equationSystem.Matrix[v][j])
                    );
            }

            expression = ExpressionHelper.Divide(expression, equationSystem.Matrix[v][v]);
            
            Console.WriteLine($"VAR: {vr.GetLabel()}: {expression.GetLabel()}");
            
            calculationEquations.Insert(0, expression);
        }

        /* 3. initialize initial values */

        for (var v = systemVars.Count - 1; v > -1; v--)
        {
            var variable = systemVars[v];
            
            if (variable is ExpressionDerivative derivative)
            {
                // some calculation logic based on initial charges / linkages
                derivative.Variable.Value = 0;
            }
        }
        
        /* 4. solve equation system */
        var dt = 0.001; // 1ms
        
        for (var i = 0; i < 100; i++)
        {
            /* 4.1. solve equations */
            for (var v = systemVars.Count - 1; v > -1; v--)
            {
                var variable = systemVars[v];
                var equation = calculationEquations[v];
                var varArray = arraysHolder[variable];
                
                var value = equation.Value; // calculation
                variable.Value = value;
                varArray.Add(value);
            }
            
            /* 4.2. find values by integration */
            foreach (var variable in systemVars)
            {
                if (variable is ExpressionDerivative derivative)
                {
                    var integralVariable = derivative.Variable;
                    integralVariable.Value = integralVariable.Value + derivative.Value * dt;
                }
            }
        }

        for (var v = systemVars.Count - 1; v > -1; v--)
        {
            var variable = systemVars[v];
            var varArray = arraysHolder[variable];

            Console.WriteLine("___________________");
            Console.WriteLine(variable.GetLabel());
            
            foreach (var value in varArray)
            {
                Console.WriteLine(value);
            }
        }
        
        TestReadyCallback?.Invoke(arraysHolder[systemVars[0]]);
    }
}