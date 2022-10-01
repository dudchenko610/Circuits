using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Events;
using Circuits.ViewModels.Math;
using Circuits.ViewModels.Rendering.Scheme;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using Circuits.Services.Services.Interfaces;

namespace Circuits.Components.Main.SchemeRenderer.Elements.WireE;

public partial class WireComponent : IDisposable
{
    [Inject] private IHighlightService _highlightService { get; set; } = null!;
    [CascadingParameter(Name="SchemeRenderReference")] public SchemeRendererComponent SchemeRenderer { get; set; } = null!;
    [Parameter] public Wire Wire { get; set; } = null!;

    private int CellSize => SchemeRendererContext.CellSize;
    private Element _selectedElement => SchemeRenderer?.SelectedElement!;
    private Element _draggingElement => SchemeRenderer?.DraggingElement!;
    private Vec2 _draggingPos => SchemeRenderer?.DraggingPos!;
    private bool _firstDragOver => SchemeRenderer == null ? false : SchemeRenderer.FirstDragOver;
    private bool _dragAllowed => SchemeRenderer.SchemeRendererContext.PencilMode;
    private bool _eventsDisabled => SchemeRenderer.SchemeRendererContext.PencilMode;

    private NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    private Vec2 _pos = new();
    
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
        if (_draggingElement == Wire && _firstDragOver)
        {
            StateHasChanged();
        }
    }

    private void OnDragStart(ExtMouseEventArgs e)
    {
        SchemeRenderer.OnDragStart(e, Wire);
    }
    
    private void OnDragEnd(ExtMouseEventArgs e)
    {
        SchemeRenderer.OnDragEnd(e);
    }
    
    private async Task OnElementClickedAsync()
    {
        await SchemeRenderer.OnElementClickedAsync(Wire);
    }

    private Vec2 GetPosition(bool subtractP1 = false)
    {
        if (subtractP1)
        {
            _pos.Set(Wire.P2).Subtract(Wire.P1);
        }
        else
        {
            _pos.Set(Wire.P2);
            _pos.Add(Wire.P1);
        }

        _pos.Multiply(0.5f * CellSize);

        var width = GetWidth();
        
        _pos.Add(-0.5f * width, 0);
        
        return _pos;
    }

    private int GetWidth()
    {
        var width = (int) Math.Abs(Wire.P2.X * CellSize - Wire.P1.X * CellSize);
        var height = (int) Math.Abs(Wire.P2.Y * CellSize - Wire.P1.Y * CellSize);
        
        return width > height ? width : height;
    }
}