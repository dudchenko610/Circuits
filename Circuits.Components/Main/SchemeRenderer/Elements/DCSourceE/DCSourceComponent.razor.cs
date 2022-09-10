using System.Globalization;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Events;
using Circuits.ViewModels.Math;
using Circuits.ViewModels.Rendering.Scheme;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeRenderer.Elements.DCSourceE;

public partial class DCSourceComponent : IDisposable
{
    [CascadingParameter(Name="SchemeRenderReference")] public SchemeRendererComponent SchemeRenderer { get; set; } = null!;
    [Parameter] public DCSource DCSource { get; set; } = null!;

    private int CellSize => SchemeRendererContext.CellSize;
    private Element _selectedElement => SchemeRenderer?.SelectedElement!;
    private Element _draggingElement => SchemeRenderer?.DraggingElement!;
    private Vec2 _draggingPos => SchemeRenderer?.DraggingPos!;
    private bool _firstDragOver => SchemeRenderer == null ? false : SchemeRenderer.FirstDragOver;
    private bool _dragAllowed => SchemeRenderer.SchemeRendererContext.PencilMode;
    private bool _eventsDisabled => SchemeRenderer.SchemeRendererContext.PencilMode;

    private NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    private bool _horizontal = false;

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
        if (_draggingElement == DCSource && _firstDragOver)
        {
            StateHasChanged();
        }    
    }

    protected override void OnParametersSet()
    {
        _horizontal = (int)DCSource.P1.Y == (int)DCSource.P2.Y;
    }

    private void OnDragStart(ExtMouseEventArgs e)
    {
        SchemeRenderer.OnDragStart(e, DCSource);
    }
    
    private void OnDragEnd(ExtMouseEventArgs e)
    {
        SchemeRenderer.OnDragEnd(e);
    }
    
    private void OnElementClicked()
    {
        SchemeRenderer.OnElementClicked(DCSource);
    }
}