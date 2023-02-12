namespace Circuits.ViewModels.Entities.Equations;

public class ShockleyDiodeEquation : ExpressionVariable
{
    // public string DiodeIdentifier { get; set; } = string.Empty;
    public ExpressionVariable Variable { get; set; } = null!;

    public override double Value => 0.000001 * System.Math.Exp(Variable.Value / 0.026 - 1);

    // public override string Label => $"i<i></i><sub-i>{DiodeIdentifier}</sub-i>({Variable.Label})";
}