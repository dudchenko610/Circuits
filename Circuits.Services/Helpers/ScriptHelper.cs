using Circuits.Services.Extensions;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Helpers;

namespace Circuits.Services.Helpers;

public static class ScriptHelper
{
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
    
    public static string BuildCalculatingJs(EquationSystem equationSystem, int iterationNumber, float dt)
    {
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

            for (var j = v + 1; j < systemVars.Count; j++)
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
            jsScript +=
                $"{{\n\tvariable: {jsVarMap[systemVars[v]]},\n\tfunc: getValOf_{jsVarMap[systemVars[v]]},\n\tarray: []\n}},\n";
        }

        jsScript = jsScript.Remove(jsScript.Length - 2, 2);
        jsScript += "]; \n\n";

        jsScript += $@"
const dt = {dt};

export function testIntegration() {{

    for (var i = 0; i < {iterationNumber}; i++)
    {{
        systemVars.forEach(varInfo => {{
            const value = varInfo.func(); // calculation
            varInfo.variable.value = value;
            varInfo.array.push(value);
        }});

        systemVars.forEach(varInfo => {{
            if (varInfo.variable.type === 'ExpressionDerivative')
            {{
                const derivative = varInfo.variable;
                const integralVariable = derivative.variable;
                integralVariable.value += derivative.value * dt;
            }}
        }});
    }}

    console.log(systemVars);
}}
        ";

        return jsScript;
    }
}