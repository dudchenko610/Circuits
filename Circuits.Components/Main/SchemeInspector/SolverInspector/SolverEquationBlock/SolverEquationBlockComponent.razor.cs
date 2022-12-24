using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Solver;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeInspector.SolverInspector.SolverEquationBlock;

public partial class SolverEquationBlockComponent : IDisposable
{
    [Inject] private ISolverService SolverService { get; set; } = null!;
    
    [Parameter] public EquationSystem EquationSystem { get; set; } = null!;

    protected override void OnInitialized()
    {
        SolverService.OnUpdate += OnUpdate;
    }

    public void Dispose()
    {
        SolverService.OnUpdate -= OnUpdate;
    }

    private void OnUpdate(EquationSystem equationSystem, EquationSystemSolverState _)
    {
        if (equationSystem == EquationSystem) StateHasChanged(); 
    }

    private async Task RunSolverAsync()
    {
        await SolverService.RunAsync(EquationSystem);
    }
    
    
    private async Task StopSolverAsync()
    {
        await SolverService.StopAsync(EquationSystem);
    }
}