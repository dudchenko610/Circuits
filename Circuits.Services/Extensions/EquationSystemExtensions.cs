using Circuits.ViewModels.Entities.Equations;

namespace Circuits.Services.Extensions;

public static class EquationSystemExtensions
{
    public static HashSet<ExpressionVariable> GetAllVariables(this EquationSystem equationSystem)
    {
        var allVariables = new HashSet<ExpressionVariable>();

        foreach (var variable in equationSystem.Variables)
        {
            allVariables.Add(variable);
        }
        
        // collect vars in right-side column
        foreach (var row in equationSystem.Matrix)
        {
            var rightColumnExpression = row[equationSystem.Matrix.Length];
            rightColumnExpression.CollectVariables(allVariables);
        }
        
        return allVariables;
    }
    
    private static void CollectVariables(this Expression expression, ISet<ExpressionVariable> variables)
    {
        if (expression is ExpressionValue) return;
        
        if (expression is ExpressionVariable expVar)
        {
            variables.Add(expVar);
            return;
        }
        
        if (expression is ExpressionAdditions expAdd)
        {
            foreach (var t in expAdd.Nodes)
            {
                t.CollectVariables(variables);
            }

            return;
        }

        if (expression is not ExpressionMultipliers expMul) return;
        {
            foreach (var t in expMul.Nodes)
            {
                t.CollectVariables(variables);
            }
        }
    }
}