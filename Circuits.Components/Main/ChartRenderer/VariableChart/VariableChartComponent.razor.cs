using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Charts;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Solver;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.ChartRenderer.VariableChart;

public partial class VariableChartComponent : IDisposable
{
    [Inject] public ISolverService SolverService { get; set; } = null!;
    [Inject] public IChartService ChartService { get; set; } = null!;
    [Parameter] public ChartInfo ChartInfo { get; set; } = null!;

    private List<float> _data = null!;

    protected override void OnInitialized()
    {
        _data = ChartInfo.SolverState.DataArrays[ChartInfo.Variable];
        SolverService.OnUpdate += Update;
    }

    public void Dispose()
    {
        SolverService.OnUpdate -= Update;
    }

    private void CloseGraph()
    {
        ChartService.Close(ChartInfo.Variable);
    }

    private void Update(EquationSystem equationSystem, EquationSystemSolverState systemSolverState)
    {
        if (equationSystem.Variables.Contains(ChartInfo.Variable))
        {
            StateHasChanged();
        }
    }
}