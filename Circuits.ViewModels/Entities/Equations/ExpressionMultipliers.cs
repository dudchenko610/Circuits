namespace Circuits.ViewModels.Entities.Equations;

public class ExpressionMultipliers : Expression
{
    public List<MathOperation> Multipliers { get; } = new();
    public List<Expression> Nodes { get; } = new();
    
    public override double Value
    {
        get
        {
            var result = 1d;

            for (var i = 0; i < Nodes.Count; i ++)
            {
                if (Multipliers[i] == MathOperation.Multiply)
                {
                    result *= Nodes[i].Value;
                }
                else
                {
                    result /= Nodes[i].Value;
                }
            }

            return result;
        }
    }
}