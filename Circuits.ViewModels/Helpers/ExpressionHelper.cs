using Circuits.ViewModels.Entities.Equations;

namespace Circuits.ViewModels.Helpers;

public static class ExpressionHelper
{
    public static Expression Add(Expression ex1, Expression ex2)
    {
        return Add(ex1, ex2, MathOperation.Plus);
    }
    
    public static Expression Subtract(Expression ex1, Expression ex2)
    {
        return Add(ex1, ex2, MathOperation.Minus);
    }

    public static Expression Multiply(Expression ex1, Expression ex2)
    {
        return Multiply(ex1, ex2, MathOperation.Multiply);
    }
    
    public static Expression Divide(Expression ex1, Expression ex2)
    {
        return Multiply(ex1, ex2, MathOperation.Divide);
    }

    private static Expression Add(Expression ex1, Expression ex2, MathOperation operation)
    {
        if (ex1 is ExpressionValue { Value: 0 })
        {
            return ex2;
        }
        
        if (ex2 is ExpressionValue { Value: 0 })
        {
            return ex1;
        }
        
        if (ex1 is ExpressionValue && ex2 is ExpressionValue)
        {
            return new ExpressionValue(ex1.Value + ex2.Value);
        }

        var additions = new ExpressionAdditions();
        
        if (ex1 is ExpressionAdditions exAdd1 && ex2 is ExpressionAdditions exAdd2)
        {
            additions.Signs.AddRange(exAdd1.Signs);
            additions.Signs.Add(operation);
            additions.Signs.AddRange(exAdd2.Signs);
            
            additions.Nodes.AddRange(exAdd1.Nodes);
            additions.Nodes.AddRange(exAdd1.Nodes);

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

    private static Expression Multiply(Expression ex1, Expression ex2, MathOperation operation)
    {
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

        var multipliers = new ExpressionMultipliers();
        
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
}