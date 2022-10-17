namespace Circuits.ViewModels.Entities.Equations;

public class ExpressionDerivative : ExpressionVariable
{
    public ExpressionVariable Variable { get; set; }
    public override string Label => $"d({Variable.Label})/dt";
}