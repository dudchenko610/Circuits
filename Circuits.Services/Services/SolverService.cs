using Circuits.Services.Extensions;
using Circuits.Services.Helpers;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Solver;
using Circuits.ViewModels.Helpers;
using Microsoft.JSInterop;

namespace Circuits.Services.Services;

public class SolverService : ISolverService
{
    public Dictionary<EquationSystem, EquationSystemSolverState> SolverState { get; } = new();
    public Action? OnClear { get; set; }
    
    private readonly IJSUtilsService _jsUtilsService;
    private readonly IJSRuntime _jsRuntime;
    
    public SolverService(IJSUtilsService jsUtilsService, IJSRuntime jsRuntime)
    {
        _jsUtilsService = jsUtilsService;
        _jsRuntime = jsRuntime;
    }
    
    public async Task<EquationSystemSolverState> RunSolverAsync(EquationSystem equationSystem)
    {
        var scriptJs = ScriptHelper.BuildCalculatingJs(equationSystem, 100, 0.001f);
        var url = await _jsUtilsService.CreateObjectURLAsync(scriptJs);

        Console.WriteLine($"URL: {url}");
        
        var jsObject = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", url);
        await jsObject.InvokeVoidAsync("testIntegration");
        await jsObject.DisposeAsync();
        
        await _jsUtilsService.RevokeObjectAsync(url);
        
        return null!;
    }


    public void Clear()
    {
        SolverState.Clear();
        OnClear?.Invoke();
    }

    private static void DotNetSolver(EquationSystem equationSystem)
    {
        if (equationSystem == null!) return;

        var systemVars = equationSystem.Variables;

        // setup arrays
        var arraysHolder = new Dictionary<ExpressionVariable, List<double>>();
        
        // _schemeService.SolverResult.Clear();
        // _schemeService.SolverResult.Add(equationSystem, arraysHolder);

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
                    integralVariable.Value += derivative.Value * dt;
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
    }
}