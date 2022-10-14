namespace Circuits.ViewModels.Entities.Equations;

public class ExpressionAdditions : Expression
{
    public List<MathOperation> Signs { get; } = new();
    public List<Expression> Nodes { get; } = new();
    
    public override double Value
    {
        get
        {
            return Nodes.Select((t, i) => (Signs[i] == MathOperation.Plus ? 1 : -1) * t.Value).Sum();
        }
    }
}