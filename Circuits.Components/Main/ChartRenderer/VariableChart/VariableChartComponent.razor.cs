using System.Globalization;
using Circuits.Components.Common.Modal;
using Circuits.Components.Common.Models.Zoom;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Charts;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Solver;
using Circuits.ViewModels.Events;
using Circuits.ViewModels.Math;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Circuits.Components.Main.ChartRenderer.VariableChart;

public partial class VariableChartComponent : IAsyncDisposable
{
    [Inject] public ISolverService SolverService { get; set; } = null!;
    [Inject] public IChartService ChartService { get; set; } = null!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = null!;
    [Parameter] public ChartInfo ChartInfo { get; set; } = null!;

    private List<float> _data = null!;
    private readonly string _controlPanelId = $"_id_{Guid.NewGuid()}";
    private readonly string _subscriptionKey = $"_id_{Guid.NewGuid()}";
    
    private DotNetObjectReference<VariableChartComponent> _dotNetRef = null!;
    private readonly NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    private readonly Vec2 _lastMousePosition = new();
    private readonly Vec2 _pos = new(100, 100);
    private bool _dragStarted = false;

    private BCHModal _bchModal = null!;

    protected override async Task OnInitializedAsync()
    {
        _data = ChartInfo.SolverState.DataArrays[ChartInfo.Variable];
        SolverService.OnUpdate += Update;
        
        _dotNetRef = DotNetObjectReference.Create(this);
        
        await JsRuntime.InvokeVoidAsync("addDocumentListener", _subscriptionKey, "mousemove", _dotNetRef,
            "OnDocumentMouseMove");
        await JsRuntime.InvokeVoidAsync("addDocumentListener", _subscriptionKey, "mouseup", _dotNetRef,
            "OnMouseLeaveUp");
    }

    public async ValueTask DisposeAsync()
    {
        SolverService.OnUpdate -= Update;
        
        await JsRuntime.InvokeVoidAsync("removeDocumentListener", _subscriptionKey, "mousemove");
        await JsRuntime.InvokeVoidAsync("removeDocumentListener", _subscriptionKey, "mouseup");
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
    
    private void OnMouseDown(ExtMouseEventArgs e)
    {
        var coordsHolder = e.PathCoordinates.FirstOrDefault();
        if (coordsHolder?.Id != _controlPanelId) return;
        
        _lastMousePosition.Set(e.PageX, e.PageY);
        _dragStarted = true;
        
        StateHasChanged();
    }
    
    [JSInvokable]
    public void OnDocumentMouseMove(ExtMouseEventArgs e)
    {
        if (!_dragStarted) return;
        
        var mousePosition = new Vec2(e.PageX, e.PageY);
        var change = new Vec2(
            mousePosition.X - _lastMousePosition.X,
            mousePosition.Y - _lastMousePosition.Y
        );

        _lastMousePosition.Set(mousePosition);
        _pos.Add(change);
        
        StateHasChanged();
    }
    
    [JSInvokable]
    public void OnMouseLeaveUp()
    {
        _dragStarted = false;

        StateHasChanged();
    }
}