namespace Circuits.ViewModels.Entities.Equations;

public class ExpressionValue : Expression
{
    public ExpressionValue(double value = 0)
    {
        Value = value;
    }

    public override double Value { get; set; }
    
    public static explicit operator ExpressionValue(double value) => new (value);
}