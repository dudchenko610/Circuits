namespace Circuits.ViewModels.Entities.Equations;

public class NonlinearExpression : Expression
{
    // TODO: Maybe data-array case should be added but for now it is just polynomial simplification or smth like that
    public ExpressionVariable Variable { get; set; } = null!;
    public override double Value { get; set; }
    public string Label { get; set; } = string.Empty;
}