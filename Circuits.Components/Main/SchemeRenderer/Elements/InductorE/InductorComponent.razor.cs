using System.Globalization;
using BlazorComponentHeap.Shared.Models.Events;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Math;
using Circuits.ViewModels.Rendering.Scheme;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Circuits.Components.Main.SchemeRenderer.Elements.InductorE;

public partial class InductorComponent
{
    [Inject] private IHighlightService HighlightService { get; set; } = null!;
    
    [CascadingParameter(Name="SchemeRenderReference")] public SchemeRendererComponent SchemeRenderer { get; set; } = null!;
    [Parameter] public Inductor Inductor { get; set; } = null!;

    private int CellSize => SchemeRendererContext.CellSize;
    private Element _selectedElement => SchemeRenderer?.SelectedElement!;
    private Element _draggingElement => SchemeRenderer?.DraggingElement!;
    private Vec2 _draggingPos => SchemeRenderer?.DraggingPos!;
    private bool _firstDragOver => SchemeRenderer == null ? false : SchemeRenderer.FirstDragOver;
    private bool _dragAllowed => SchemeRenderer.SchemeRendererContext.PencilMode;
    private bool _eventsDisabled => SchemeRenderer.SchemeRendererContext.PencilMode;

    private NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    private Vec2 _pos = new();
    private Vec2 _prevDragPos = new();

    // protected override void OnAfterRender(bool firstRender)
    // {
    //     Console.WriteLine("OnAfterRender Wire");
    // }

    protected override void OnInitialized()
    {
        SchemeRenderer.OnDragUpdate += OnDraggingUpdate;
        HighlightService.OnUpdate += StateHasChanged;
    }

    public void Dispose()
    {
        SchemeRenderer.OnDragUpdate -= OnDraggingUpdate;
        HighlightService.OnUpdate -= StateHasChanged;
    }

    private void OnDraggingUpdate()
    {
        if (_draggingElement == Inductor && _firstDragOver)
        {
            if (_draggingPos.X != _prevDragPos.X || _draggingPos.Y != _prevDragPos.Y)
            {
                _prevDragPos.Set(_draggingPos);
                StateHasChanged();
            }
        }    
    }

    private void OnDragStart(ExtMouseEventArgs e)
    {
        SchemeRenderer.OnDragStart(e, Inductor);
    }
    
    private void OnDragEnd(ExtMouseEventArgs e)
    {
        SchemeRenderer.OnDragEnd(e);
    }
    
    private async Task OnElementClickedAsync(MouseEventArgs e)
    {
        await SchemeRenderer.OnElementClickedAsync(Inductor,new Vec2(e.PageX, e.PageY));
    }
    
    private Vec2 GetPosition(bool subtractP1 = false)
    {
        if (subtractP1)
        {
            _pos.Set(Inductor.P2).Subtract(Inductor.P1);
        }
        else
        {
            _pos.Set(Inductor.P2);
            _pos.Add(Inductor.P1);
        }

        _pos.Multiply(0.5f * CellSize);

        var width = GetWidth();
        
        _pos.Add(-0.5f * width, 0);
        
        return _pos;
    }

    private int GetWidth()
    {
        var width = (int) Math.Abs(Inductor.P2.X * CellSize - Inductor.P1.X * CellSize);
        var height = (int) Math.Abs(Inductor.P2.Y * CellSize - Inductor.P1.Y * CellSize);
        
        return width > height ? width : height;
    }
}