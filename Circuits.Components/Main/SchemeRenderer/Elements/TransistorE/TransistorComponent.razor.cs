using System.Globalization;
using Circuits.Components.Common.Events;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Math;
using Circuits.ViewModels.Rendering.Scheme;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Circuits.Components.Main.SchemeRenderer.Elements.TransistorE;

public partial class TransistorComponent : IDisposable
{
    [Inject] private IHighlightService _highlightService { get; set; } = null!;
    
    [CascadingParameter(Name = "SchemeRenderReference")]
    public SchemeRendererComponent SchemeRenderer { get; set; } = null!;

    [Parameter] public BipolarTransistor BipolarTransistor { get; set; } = null!;

    private int CellSize => SchemeRendererContext.CellSize;
    private Element _selectedElement => SchemeRenderer?.SelectedElement!;
    private Element _draggingElement => SchemeRenderer?.DraggingElement!;
    private Vec2 _draggingPos => SchemeRenderer?.DraggingPos!;
    private bool _firstDragOver => SchemeRenderer == null ? false : SchemeRenderer.FirstDragOver;
    private bool _dragAllowed => SchemeRenderer.SchemeRendererContext.PencilMode;
    private bool _eventsDisabled => SchemeRenderer.SchemeRendererContext.PencilMode;

    private NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    // private bool _horizontal = false;

    // protected override void OnAfterRender(bool firstRender)
    // {
    //     Console.WriteLine("OnAfterRender Wire");
    // }

    protected override void OnInitialized()
    {
        SchemeRenderer.OnDragUpdate += OnDraggingUpdate;
        _highlightService.OnUpdate += StateHasChanged;
    }

    public void Dispose()
    {
        SchemeRenderer.OnDragUpdate -= OnDraggingUpdate;
        _highlightService.OnUpdate -= StateHasChanged;
    }

    private void OnDraggingUpdate()
    {
        if (_draggingElement == BipolarTransistor && _firstDragOver) StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        //_horizontal = (int)Resistor.P1.Y == (int)Resistor.P2.Y;
    }

    private void OnDragStart(ExtMouseEventArgs e)
    {
        SchemeRenderer.OnDragStart(e, BipolarTransistor);
    }

    private void OnDragEnd(ExtMouseEventArgs e)
    {
        SchemeRenderer.OnDragEnd(e);
    }

    private async Task OnElementClickedAsync(MouseEventArgs e)
    {
        await SchemeRenderer.OnElementClickedAsync(BipolarTransistor, new Vec2(e.PageX, e.PageY));
    }
}