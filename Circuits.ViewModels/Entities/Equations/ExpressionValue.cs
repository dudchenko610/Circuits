namespace Circuits.ViewModels.Entities.Equations;

public class ExpressionValue : Expression
{
    public ExpressionValue(double value = 0)
    {
        Value = value;
    }

    public override double Value { get; }
    
    public static explicit operator ExpressionValue(double value) => new (value);
}