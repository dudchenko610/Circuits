using Circuits.ViewModels.Helpers;

namespace Circuits.ViewModels.Entities.Equations;

public abstract class Expression
{
    public object Payload { get; set; } = null!;
    public abstract double Value { get; set; }
    
    public static Expression operator +(Expression e1, Expression e2) => ExpressionHelper.Add(e1, e2);
    public static Expression operator +(Expression e1, double v2) => ExpressionHelper.Add(e1, new ExpressionValue(v2));
    public static Expression operator +(double v1, Expression e2) => ExpressionHelper.Add(new ExpressionValue(v1), e2);
    
    public static Expression operator -(Expression e1, Expression e2) => ExpressionHelper.Subtract(e1, e2);
    public static Expression operator -(Expression e1, double v2) => ExpressionHelper.Subtract(e1, new ExpressionValue(v2));
    public static Expression operator -(double v1, Expression e2) => ExpressionHelper.Subtract(new ExpressionValue(v1), e2);
    
    public static Expression operator *(Expression e1, Expression e2) => ExpressionHelper.Multiply(e1, e2);
    public static Expression operator *(Expression e1, double v2) => ExpressionHelper.Multiply(e1, new ExpressionValue(v2));
    public static Expression operator *(double v1, Expression e2) => ExpressionHelper.Multiply(new ExpressionValue(v1), e2);
    
    public static Expression operator /(Expression e1, Expression e2) => ExpressionHelper.Divide(e1, e2);
    public static Expression operator /(Expression e1, double v2) => ExpressionHelper.Divide(e1, new ExpressionValue(v2));
    public static Expression operator /(double v1, Expression e2) => ExpressionHelper.Divide(new ExpressionValue(v1), e2);
    
    // unary
    public static Expression operator -(Expression e) => ExpressionHelper.Multiply(new ExpressionValue(-1), e);
}