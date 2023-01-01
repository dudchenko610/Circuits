using Circuits.ViewModels.Entities.Structures;

namespace Circuits.ViewModels.Entities.Equations;

public class EquationSystem
{
    public IReadOnlyList<ExpressionVariable> Variables => _variables;
    public Expression[][] Matrix { get; init; }

    private readonly List<ExpressionVariable> _variables = new();

    public EquationSystem(params ExpressionVariable[] variables)
    {
        _variables.AddRange(variables);
        Matrix = new Expression[variables.Length][];
        
        for (var i = 0; i < variables.Length; i ++)
        {
            Matrix[i] = new Expression[variables.Length + 1];
        }
    }
}