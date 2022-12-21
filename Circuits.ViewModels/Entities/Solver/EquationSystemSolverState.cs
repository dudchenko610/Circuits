using Circuits.ViewModels.Entities.Equations;

namespace Circuits.ViewModels.Entities.Solver;

public class EquationSystemSolverState
{
    public Dictionary<ExpressionVariable, List<double>> DataArrays { get; set; } = new();
    public int IterationCount { get; set; } = 100;
    public float DeltaTime { get; set; } = 0.001f;
    public bool IsInProgress { get; set; } = true;
    public Action? Update { get; set; } = null!;
    
    public IAsyncDisposable JsObjectReference { get; set; } = null!;
    public string ScriptUrl { get; set; } = string.Empty;
}