using System.Globalization;
using BlazorComponentHeap.Components.Modal;
using BlazorComponentHeap.Shared.Models.Events;
using Circuits.Components.Common.Models.Zoom;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Charts;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Solver;
using Circuits.ViewModels.Entities.Structures;
using Circuits.ViewModels.Math;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Circuits.Components.Main.ChartRenderer.VariableChart;

public partial class VariableChartComponent : IDisposable
{
    private static int _chartZIndex = 999999;

    [Inject] public IWorkerService WorkerService { get; set; } = null!;
    [Inject] public IChartService ChartService { get; set; } = null!;
    [Inject] public ISchemeService SchemeService { get; set; } = null!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = null!;
    [Parameter] public ChartInfo ChartInfo { get; set; } = null!;

    private List<float> _data = null!;
    private readonly string _chartContainerId = $"_id_{Guid.NewGuid()}";
    private readonly string _controlPanelId = $"_id_{Guid.NewGuid()}";
    private readonly string _subscriptionKey = $"_id_{Guid.NewGuid()}";

    protected override void OnInitialized()
    {
        _data = ChartInfo.SolverState.DataArrays[ChartInfo.Variable];
        WorkerService.OnUpdate += Update;
    }

    public void Dispose()
    {
        WorkerService.OnUpdate -= Update;
    }

    protected override void OnParametersSet()
    {
        _data = ChartInfo.SolverState.DataArrays[ChartInfo.Variable];
        StateHasChanged();
    }

    private void CloseGraph()
    {
        ChartService.Close(ChartInfo.Variable);
    }

    private void Update(EquationSystem equationSystem, EquationSystemSolverState systemSolverState)
    {
        if (equationSystem.Variables.Contains(ChartInfo.Variable) || 
            equationSystem.Variables.OfType<ExpressionDerivative>().Any(x => x.Variable == ChartInfo.Variable))
        {
            StateHasChanged();
        }

        // Console.WriteLine("Update(EquationSystem equationSystem, EquationSystemSolverState systemSolverState)");
    }

    private string GetInfoLabel()
    {
        if (ChartInfo.Key is Branch branch)
        {
            var graph = SchemeService.Graphs.FirstOrDefault(g =>
                g.Circuits.FirstOrDefault(c => c.Branches.Contains(branch)) != null);
            
            if (graph == null!) return string.Empty;
            var branches = graph.Circuits.SelectMany(x => x.Branches).ToList();
            var index = branches.IndexOf(branch);
            
            return $"Current i<i>{index}</i>";
        }

        return ChartInfo.Key switch
        {
            Capacitor capacitor => $"Capacitor C<i>{capacitor.Number}</i>",
            Inductor inductor => $"Inductor L<i>{inductor.Number}</i>",
            Resistor resistor => $"Resistor R<i>{resistor.Number}</i>",
            _ => ""
        };
    }
}