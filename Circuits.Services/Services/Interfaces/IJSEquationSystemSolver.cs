namespace Circuits.Services.Services.Interfaces;

public interface IJSEquationSystemSolver
{
    Action<List<double>>? TestReadyCallback { get; set; }
    void BuildJsFunctions();
    Task TestSolveAsync();
}