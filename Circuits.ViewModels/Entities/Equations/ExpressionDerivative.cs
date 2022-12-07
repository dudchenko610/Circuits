namespace Circuits.ViewModels.Entities.Equations;

public class ExpressionDerivative : ExpressionVariable
{
    public ExpressionVariable Variable { get; set; } = null!;
    public override string Label => $"d({Variable.Label})/dt";
}