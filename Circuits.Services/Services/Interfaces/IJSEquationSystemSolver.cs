namespace Circuits.Services.Services.Interfaces;

public interface IJSEquationSystemSolver
{
    Action<List<double>>? TestReadyCallback { get; set; }
    Task BuildJsFunctionsAsync();
    Task TestSolveAsync();
}