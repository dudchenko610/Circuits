using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Solver;

namespace Circuits.Services.Services.Interfaces;

public interface ISolverService
{
    Dictionary<EquationSystem, EquationSystemSolverState> SolverState { get; }
    Task<EquationSystemSolverState> RunSolverAsync(EquationSystem equationSystem);
    Action? OnClear { get; set; }
    void Clear();
}