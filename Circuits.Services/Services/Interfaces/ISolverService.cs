using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Solver;

namespace Circuits.Services.Services.Interfaces;

public interface ISolverService
{
    Action<EquationSystem, EquationSystemSolverState>? OnUpdate { get; set; }
    Action? OnClear { get; set; }
    
    Dictionary<EquationSystem, EquationSystemSolverState> SolverState { get; }
    Task RunAsync(EquationSystem equationSystem);
    Task StopAsync(EquationSystem equationSystem);
    Task ClearAsync();
}