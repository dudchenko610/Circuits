using Circuits.ViewModels.Entities.Equations;

namespace Circuits.ViewModels.Entities.Solver;

public class EquationSystemSolverState
{
    public Dictionary<ExpressionVariable, List<float>> DataArrays { get; set; } = new();
    public int IterationCount { get; set; } = 100;
    public float DeltaTime { get; set; } = 0.001f;
    public string Status { get; set; } = string.Empty;
    public string ScriptUrl { get; set; } = string.Empty;
}