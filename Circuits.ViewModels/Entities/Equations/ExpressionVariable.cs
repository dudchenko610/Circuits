namespace Circuits.ViewModels.Entities.Equations;

public class ExpressionVariable : Expression
{
    public virtual string Label { get; set; } = string.Empty;
    public override double Value { get; } = 0.0;
}