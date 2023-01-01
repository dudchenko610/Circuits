using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Solver;

namespace Circuits.ViewModels.Entities.Charts;

public class ChartInfo
{
    public ExpressionVariable Variable { get; set; } = null!;
    public EquationSystem EquationSystem { get; set; } = null!;
    public EquationSystemSolverState SolverState { get; set; } = null!;
    public Func<float, float> FuncModifier { get; set; } = x => x;
    public string VerticalLetter { get; set; } = string.Empty;
}