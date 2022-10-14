namespace Circuits.ViewModels.Entities.Equations;

public class ExpressionMultipliers : Expression
{
    public List<bool> Multipliers { get; } = new();
    public List<Expression> Nodes { get; } = new();
    
    public override double Value
    {
        get
        {
            var result = 1d;

            for (var i = 0; i < Nodes.Count; i ++)
            {
                if (Multipliers[i])
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