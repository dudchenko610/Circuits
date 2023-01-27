namespace Circuits.ViewModels.Entities.Equations;

public class EquationSystem
{
    public IReadOnlyList<ExpressionVariable> Variables { get; } = new List<ExpressionVariable>();
    public Expression[][] Matrix { get; init; }

    public EquationSystem(params ExpressionVariable[] variables)
    {
        ((List<ExpressionVariable>) Variables).AddRange(variables);
        Matrix = new Expression[variables.Length][];
        
        for (var i = 0; i < variables.Length; i ++)
        {
            Matrix[i] = new Expression[variables.Length + 1];

            for (var j = 0; j < variables.Length + 1; j ++)
            {
                Matrix[i][j] = new ExpressionValue();
            }
        }
    }
}