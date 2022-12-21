using Circuits.Services.Extensions;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Helpers;
using Microsoft.JSInterop;

namespace Circuits.Services.Services;

public class JSEquationSystemSolver : IJSEquationSystemSolver
{
    public Action<List<double>>? TestReadyCallback { get; set; }
    
    private readonly ISchemeService _schemeService;
    private readonly IJSUtilsService _jsUtilsService;
    private readonly IJSRuntime _jsRuntime;
    
    public JSEquationSystemSolver(
        ISchemeService schemeService, 
        IJSUtilsService jsUtilsService,
        IJSRuntime jsRuntime)
    {
        _schemeService = schemeService;
        _jsUtilsService = jsUtilsService;
        _jsRuntime = jsRuntime;
    }

    private static int GetVariableOrder(Expression expression)
    {
        var c = 0;
        var derivative = expression as ExpressionDerivative;
        
        while (derivative is not null)
        {
            c++;
            derivative = derivative.Variable as ExpressionDerivative;
        }
        
        return c;
    }

    public async Task BuildJsFunctionsAsync()
    {
        var equationSystem = _schemeService.EquationSystems.FirstOrDefault();
        if (equationSystem == null!) return;

        var allVariables = equationSystem
            .GetAllVariables()
            .OrderBy(GetVariableOrder)
            .ToList();
        var systemVars = equationSystem.Variables;
        
        var jsScript = "";
        var varCounter = 0;
        var jsVarMap = new Dictionary<ExpressionVariable, string>();

        foreach (var variable in allVariables)
        {
            var jsVar = $"v_{varCounter++}";
            jsVarMap.Add(variable, jsVar);
        }
        
        /* 1. set initial values for variables with derivatives */
        foreach (var variable in allVariables)
        {
            var jsVar = jsVarMap[variable];
            
            jsScript += $"const {jsVar} = ";
            
            if (variable is ExpressionDerivative)
            {
                // initialize based on start values
            }
            
            jsScript += $"{{ value: {variable.Value}, type: '{variable.GetType().Name}'"; 
            
            if (variable is ExpressionDerivative derivative)
            {
                jsScript += $", variable: {jsVarMap[derivative.Variable]}";
            }

            jsScript += " }; \n";
        }
        
        jsScript += "\n";

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

            jsScript += $"function getValOf_{vr.GetLabel(x => jsVarMap[x])}() {{\n";
            jsScript += $"\treturn {expression.GetLabel(x => $"{jsVarMap[x]}.value")};";
            jsScript += "\n}\n\n";
        }
        
        jsScript += "const systemVars = [\n";
        
        for (var v = systemVars.Count - 1; v > -1; v--)
        {
            jsScript += $"{{\n\tvariable: {jsVarMap[systemVars[v]]},\n\tfunc: getValOf_{jsVarMap[systemVars[v]]},\n\tarray: []\n}},\n";
        }
        
        jsScript = jsScript.Remove(jsScript.Length - 2, 2);
        jsScript += "]; \n\n";

        jsScript += @"
const dt = 0.001;

export function testIntegration() {

    for (var i = 0; i < 100; i++)
    {
        systemVars.forEach(varInfo => {
            const value = varInfo.func(); // calculation
            varInfo.variable.value = value;
            varInfo.array.push(value);
        });

        systemVars.forEach(varInfo => {
            if (varInfo.variable.type === 'ExpressionDerivative')
            {
                const derivative = varInfo.variable;
                const integralVariable = derivative.variable;
                integralVariable.value += derivative.value * dt;
            }
        });
    }

    console.log(systemVars);
}
        ";
           
        Console.WriteLine("SCRIPT:");
        Console.WriteLine(jsScript);
        
        var url = await _jsUtilsService.CreateObjectURLAsync(jsScript);

        Console.WriteLine($"URL: {url}");
        
        var jsObject = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", url);
        await jsObject.InvokeVoidAsync("testIntegration");
        await jsObject.DisposeAsync();
        
        await _jsUtilsService.RevokeObjectAsync(url);
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
        
        TestReadyCallback?.Invoke(arraysHolder[systemVars[0]]);
    }
}