namespace Circuits.ViewModels.Entities.Equations;

public class ExpressionAdditions : Expression
{
    public List<MathOperation> Signs { get; } = new();
    public List<Expression> Nodes { get; } = new();
    
    public override double Value
    {
        get
        {
            if (Nodes.Count == 0) return 0;
            var result = Nodes[0].Value;

            for (var i = 1; i < Nodes.Count; i ++)
            {
                result += ((Signs[i - 1] == MathOperation.Plus ? 1 : -1) * Nodes[i].Value);
            }

            return result;
        }
        set
        {
            
        }
    }
}