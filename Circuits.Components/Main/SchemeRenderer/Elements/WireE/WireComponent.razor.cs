using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Events;
using Circuits.ViewModels.Math;
using Circuits.ViewModels.Rendering.Scheme;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeRenderer.Elements.WireE;

public partial class WireComponent : IDisposable
{
    [CascadingParameter(Name="SchemeRenderReference")] public SchemeRendererComponent SchemeRenderer { get; set; } = null!;
    [Parameter] public Wire Wire { get; set; } = null!;

    private int CellSize => SchemeRendererContext.CellSize;
    private Element _selectedElement => SchemeRenderer?.SelectedElement;
    private Element _draggingElement => SchemeRenderer?.DraggingElement;
    private Vec2 _draggingPos => SchemeRenderer?.DraggingPos;
    private bool _firstDragOver => SchemeRenderer == null ? false : SchemeRenderer.FirstDragOver;
    
    private bool _horizontal = false;
    private string _horOffset = "0px";
    private string _verOffset = "0px";

    // protected override void OnAfterRender(bool firstRender)
    // {
    //     Console.WriteLine("OnAfterRender Wire");
    // }

    protected override void OnInitialized()
    {
        SchemeRenderer.OnDragUpdate += OnDraggingUpdate;
    }

    public void Dispose()
    {
        SchemeRenderer.OnDragUpdate -= OnDraggingUpdate;
    }

    private void OnDraggingUpdate()
    {
        if (_draggingElement == Wire && _firstDragOver)
            StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        _horizontal = (int)Wire.P1.Y == (int)Wire.P2.Y;
        _horOffset = !_horizontal ? "1.5px" : "0px";
        _verOffset = _horizontal ? "1.5px" : "0px";
    }

    private void OnDragStart(ExtMouseEventArgs e)
    {
        SchemeRenderer.OnDragStart(e, Wire);
    }
    
    private void OnDragEnd(ExtMouseEventArgs e)
    {
        SchemeRenderer.OnDragEnd(e);
    }
    
    private void OnElementClicked()
    {
        SchemeRenderer.OnElementClicked(Wire);
    }
}