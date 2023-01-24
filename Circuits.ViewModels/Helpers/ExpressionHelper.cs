using Circuits.ViewModels.Entities.Equations;

namespace Circuits.ViewModels.Helpers;

public static class ExpressionHelper
{
    public static Expression Add(Expression ex1, Expression ex2, bool simplify = true)
    {
        return Add(ex1, ex2, MathOperation.Plus, simplify);
    }
    
    public static Expression Subtract(Expression ex1, Expression ex2, bool simplify = true)
    {
        return Add(ex1, ex2, MathOperation.Minus, simplify);
    }

    public static Expression Multiply(Expression ex1, Expression ex2, bool simplify = true)
    {
        var result = Multiply(ex1, ex2, MathOperation.Multiply, simplify);
        return TryToSimplifyMultipliers(result);
    }
    
    public static Expression Divide(Expression ex1, Expression ex2, bool simplify = true)
    {
        return Multiply(ex1, ex2, MathOperation.Divide, simplify);
    }

    private static Expression Add(Expression ex1, Expression ex2, MathOperation operation, bool simplify)
    {
        var additions = new ExpressionAdditions();

        if (!simplify)
        {
            additions.Signs.Add(operation);
            additions.Nodes.Add(ex1);
            additions.Nodes.Add(ex2);
        
            return additions;
        }
        
        if (ex1 is ExpressionValue { Value: 0 })
        {
            return operation == MathOperation.Plus ? ex2 : -1 * ex2;
        }
        
        if (ex2 is ExpressionValue { Value: 0 })
        {
            return ex1;
        }
        
        if (ex1 is ExpressionValue && ex2 is ExpressionValue)
        {
            return new ExpressionValue(
                operation == MathOperation.Plus ? 
                    (ex1.Value + ex2.Value) : 
                    (ex1.Value - ex2.Value));
        }
        
        if (ex1 is ExpressionAdditions exAdd1 && ex2 is ExpressionAdditions exAdd2)
        {
            additions.Signs.AddRange(exAdd1.Signs);
            additions.Signs.Add(operation);
            additions.Signs.AddRange(exAdd2.Signs);
            
            additions.Nodes.AddRange(exAdd1.Nodes);
            additions.Nodes.AddRange(exAdd2.Nodes);

            return additions;
        }

        if (ex1 is ExpressionAdditions exAddition1 && ex2 is not ExpressionAdditions)
        {
            additions.Signs.AddRange(exAddition1.Signs);
            additions.Nodes.AddRange(exAddition1.Nodes);
            
            additions.Signs.Add(operation);
            additions.Nodes.Add(ex2);

            return additions;
        }
        
        if (ex2 is ExpressionAdditions exAddition2 && ex1 is not ExpressionAdditions)
        {
            additions.Signs.AddRange(exAddition2.Signs);
            additions.Nodes.AddRange(exAddition2.Nodes);
            
            additions.Signs.Add(operation);
            additions.Nodes.Add(ex1);

            return additions;
        }
        
        additions.Signs.Add(operation);
        additions.Nodes.Add(ex1);
        additions.Nodes.Add(ex2);
        
        return additions;
    }

    private static Expression Multiply(Expression ex1, Expression ex2, MathOperation operation, bool simplify)
    {
        var multipliers = new ExpressionMultipliers();
        
        if (!simplify)
        {
            multipliers.Multipliers.Add(operation);
            multipliers.Nodes.Add(ex1);
            multipliers.Nodes.Add(ex2);
        
            return multipliers;
        }
        
        if (operation == MathOperation.Multiply && (ex1 is ExpressionValue { Value: 0 } || ex2 is ExpressionValue { Value: 0 }))
        {
            return new ExpressionValue(0);
        }
        
        if (ex1 is ExpressionValue && ex2 is ExpressionValue)
        {
            return new ExpressionValue(
                operation == MathOperation.Multiply 
                    ? ex1.Value * ex2.Value 
                    : ex1.Value / ex2.Value);
        }

        if (ex1 is ExpressionMultipliers exMul1 && ex2 is ExpressionMultipliers exMul2)
        {
            multipliers.Multipliers.AddRange(exMul1.Multipliers);
            multipliers.Multipliers.Add(operation);
            multipliers.Multipliers.AddRange(exMul2.Multipliers);
            
            multipliers.Nodes.AddRange(exMul1.Nodes);
            multipliers.Nodes.AddRange(exMul2.Nodes);

            return multipliers;
        }

        if (ex1 is ExpressionMultipliers exMultipliers1 && ex2 is not ExpressionMultipliers)
        {
            multipliers.Multipliers.AddRange(exMultipliers1.Multipliers);
            multipliers.Nodes.AddRange(exMultipliers1.Nodes);
            
            multipliers.Multipliers.Add(operation);
            multipliers.Nodes.Add(ex2);

            return multipliers;
        }
        
        if (ex2 is ExpressionMultipliers exMultipliers2 && ex1 is not ExpressionMultipliers)
        {
            multipliers.Multipliers.AddRange(exMultipliers2.Multipliers);
            multipliers.Nodes.AddRange(exMultipliers2.Nodes);
            
            multipliers.Multipliers.Add(operation);
            multipliers.Nodes.Add(ex1);

            return multipliers;
        }
        
        multipliers.Multipliers.Add(operation);
        multipliers.Nodes.Add(ex1);
        multipliers.Nodes.Add(ex2);

        return multipliers;
    }

    private static Expression TryToSimplifyMultipliers(Expression expression)
    {
        if (expression is not ExpressionMultipliers expMult) return expression;
        
        var dividers = expMult.Multipliers.Where(x => x == MathOperation.Divide);
        if (dividers.Any()) return expression;

        var valueExpressions = expMult.Nodes.OfType<ExpressionValue>();
        var res = valueExpressions.Aggregate(1.0, (current, valExp) => current * valExp.Value);
        
        expMult.Nodes.RemoveAll(x => x is ExpressionValue);
        if (expMult.Nodes.Count == 0) return new ExpressionValue(res);
        
        expMult.Multipliers.Clear();
        expMult.Nodes.Insert(0, new ExpressionValue(res));
        
        for (var i = 0; i < expMult.Nodes.Count - 1; i ++)
        {
            expMult.Multipliers.Add(MathOperation.Multiply);
        }
        
        return expMult;
    }
}