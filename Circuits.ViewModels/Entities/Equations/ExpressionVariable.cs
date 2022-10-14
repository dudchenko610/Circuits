namespace Circuits.ViewModels.Entities.Equations;

public class ExpressionVariable : Expression
{
    public string Label { get; set; } = string.Empty;
    public override double Value { get; }
}