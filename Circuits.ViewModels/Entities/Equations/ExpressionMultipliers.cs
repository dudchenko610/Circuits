namespace Circuits.ViewModels.Entities.Equations;

public class ExpressionMultipliers : Expression
{
    public List<MathOperation> Multipliers { get; } = new();
    public List<Expression> Nodes { get; } = new();
    
    public override double Value
    {
        get
        {
            if (Nodes.Count == 0) return double.NaN;
            var result = Nodes[0].Value;

            for (var i = 1; i < Nodes.Count; i ++)
            {
                if (Multipliers[i - 1] == MathOperation.Multiply) result *= Nodes[i].Value;
                else result /= Nodes[i].Value;
            }

            return result;
        }
        
        set {}
    }
}