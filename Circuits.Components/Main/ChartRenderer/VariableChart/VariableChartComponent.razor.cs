using System.Globalization;
using Circuits.Components.Common.Events;
using Circuits.Components.Common.Modal;
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

public partial class VariableChartComponent : IAsyncDisposable
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

    private DotNetObjectReference<VariableChartComponent> _dotNetRef = null!;
    private readonly NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    private readonly Vec2 _lastMousePosition = new();
    private readonly Vec2 _pos = new(100, 100);
    private bool _dragStarted = false;

    private BCHModal _bchModal = null!;
    private int _zIndex = _chartZIndex;

    protected override async Task OnInitializedAsync()
    {
        _data = ChartInfo.SolverState.DataArrays[ChartInfo.Variable];
        WorkerService.OnUpdate += Update;

        _dotNetRef = DotNetObjectReference.Create(this);

        await JsRuntime.InvokeVoidAsync("addDocumentListener", _subscriptionKey, "mousedown", _dotNetRef,
            "OnDocumentMouseDown");
        await JsRuntime.InvokeVoidAsync("addDocumentListener", _subscriptionKey, "mousemove", _dotNetRef,
            "OnDocumentMouseMoveAsync");
        await JsRuntime.InvokeVoidAsync("addDocumentListener", _subscriptionKey, "mouseup", _dotNetRef,
            "OnMouseLeaveUp");
    }

    public async ValueTask DisposeAsync()
    {
        WorkerService.OnUpdate -= Update;

        await JsRuntime.InvokeVoidAsync("removeDocumentListener", _subscriptionKey, "mousedown");
        await JsRuntime.InvokeVoidAsync("removeDocumentListener", _subscriptionKey, "mousemove");
        await JsRuntime.InvokeVoidAsync("removeDocumentListener", _subscriptionKey, "mouseup");
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

    [JSInvokable]
    public void OnDocumentMouseDown(ExtMouseEventArgs e)
    {
        var coordsHolder = e.PathCoordinates.FirstOrDefault();

        if (e.PathCoordinates.FirstOrDefault(x => x.Id == _chartContainerId) != null)
        {
            _zIndex = ++_chartZIndex;
            if (coordsHolder?.Id != _controlPanelId) StateHasChanged();
        }

        if (coordsHolder?.Id != _controlPanelId) return;

        _lastMousePosition.Set(e.PageX, e.PageY);
        _dragStarted = true;

        StateHasChanged();
    }

    [JSInvokable]
    public async Task OnDocumentMouseMoveAsync(ExtMouseEventArgs e)
    {
        if (!_dragStarted) return;

        var mousePosition = new Vec2(e.PageX, e.PageY);
        var change = new Vec2(
            mousePosition.X - _lastMousePosition.X,
            mousePosition.Y - _lastMousePosition.Y
        );

        _lastMousePosition.Set(mousePosition);
        _pos.Add(change);

        await _bchModal.SetPositionEfficientlyAsync($"{_pos.X.ToString(_nF)}px", $"{_pos.Y.ToString(_nF)}px");
    }

    [JSInvokable]
    public void OnMouseLeaveUp()
    {
        _dragStarted = false;
        StateHasChanged();
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