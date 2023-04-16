using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Math;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Xml.Linq;
using BlazorComponentHeap.Core.Services.Interfaces;
using BlazorComponentHeap.Shared.Models.Events;
using Circuits.ViewModels.Rendering;
using Microsoft.JSInterop;

namespace Circuits.Components.Main.NavigationPlane;

public partial class NavigationPlaneComponent : IAsyncDisposable
{
    [Inject] private IJSUtilsService JsUtilsService { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Parameter] public RenderFragment<NavigationPlaneContext> ChildContent { get; set; } = null!;
    [Parameter] public NavigationPlaneContext NavigationPlaneContext { get; set; } = null!;
    [Parameter] public float Factor { get; set; } = 0.009f;
    [Parameter] public float MinScale { get; set; } = 4f;
    [Parameter] public float MaxScale { get; set; } = 5f;

    private readonly string _navigationId = $"_id_{Guid.NewGuid()}";
    private readonly string _wrapperId = $"_id_{Guid.NewGuid()}";
    private readonly string _subscriptionKey = $"_id_{Guid.NewGuid()}";
    private readonly NumberFormatInfo _nF = new () { NumberDecimalSeparator = "." };

    private Vec2 _size = new () { X = 3000, Y = 3000 };
    private Vec2 _viewPortSize = new();
    private Vec2 _pos = new();
    private float _scale = 4;
    private Vec2 _zoomTarget = new();
    private Vec2 _zoomPoint = new();
    private Vec2 _lastMousePosition = new();
    private bool _dragStarted = false;
    private float _factor = 0.3f;

    private bool _zoomKeep = false;

    private DotNetObjectReference<NavigationPlaneComponent> _dotNetRef = null!;
    
    protected override async Task OnInitializedAsync()
    {
        IJSUtilsService.OnResize += OnResizeAsync;
        NavigationPlaneContext.ZoomUp += OnZoomUp;
        NavigationPlaneContext.ZoomDown += OnZoomDownAsync;
        _dotNetRef = DotNetObjectReference.Create(this);
        
        // await JsRuntime.InvokeVoidAsync("addDocumentListener", _subscriptionKey, "mousemove", _dotNetRef,
        //     "OnDocumentMouseMove");
        // await JsRuntime.InvokeVoidAsync("addDocumentListener", _subscriptionKey, "mouseup", _dotNetRef,
        //     "OnMouseLeaveUp");
    }
    
    public async ValueTask DisposeAsync()
    {
        IJSUtilsService.OnResize -= OnResizeAsync;
        NavigationPlaneContext.ZoomUp -= OnZoomUp;
        NavigationPlaneContext.ZoomDown -= OnZoomDownAsync;
        
        // await JsRuntime.InvokeVoidAsync("removeDocumentListener", _subscriptionKey, "mousemove");
        // await JsRuntime.InvokeVoidAsync("removeDocumentListener", _subscriptionKey, "mouseup");
    }
    
    // protected override void OnAfterRender(bool firstRender)
    // {
    //     Console.WriteLine("NavigationPlane OnAfterRender");
    // }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Console.WriteLine($"OnAfterRender NavigationPlaneComponent {_dragStarted}");
        if (firstRender) await OnResizeAsync();
    }

    private async Task OnResizeAsync()
    {
        var wrapperRect = await JsUtilsService.GetBoundingClientRectAsync(_wrapperId);
        _viewPortSize.Set(wrapperRect.Width, wrapperRect.Height);

        Update();
    }

    private async Task OnZoomDownAsync(float zoomDelta)
    {
        _zoomKeep = true;
        await OnZoomHoldAsync(zoomDelta);
    }

    private async Task OnZoomHoldAsync(float zoomDelta)
    {
        if (_zoomKeep)
        {
            OnZoom(_viewPortSize.X * 0.5f, _viewPortSize.Y * 0.5f, zoomDelta);
            await Task.Delay(10);
            await OnZoomHoldAsync(zoomDelta);
        }
    }

    private void OnZoomUp()
    {
        _zoomKeep = false;
    }

    private void Update()
    {
        if (_pos.X > 0)
        {
            _pos.X = 0;
        }
        
        if (_pos.Y > 0)
        {
            _pos.Y = 0;
        }
        
        if ((_pos.X - _viewPortSize.X) / Scale < -_size.X)
        {
            _pos.X = _viewPortSize.X - _size.X * Scale;
        }
        
        if ((_pos.Y - _viewPortSize.Y) / Scale < -_size.Y)
        {
            _pos.Y = _viewPortSize.Y - _size.Y * Scale;
        }
        
        // Console.WriteLine($"X: {_size.X * _scale}, Y: {_size.Y * _scale}");
        // Console.WriteLine($"_viewPortSize: X: {_viewPortSize.X}, Y: {_viewPortSize.Y}");

        StateHasChanged();
    }

    private void OnMouseWheel(ExtWheelEventArgs e)
    {
        var container = e.PathCoordinates.FirstOrDefault(x => x.ClassList.Contains("navigation-cs-container"));

        if (container == null)
            return;

        OnZoom(container.X, container.Y, - (float)e.DeltaY);
    }

    private void OnZoom(float x, float y, float zoomDelta)
    {
        _zoomPoint.Set(x, y);
        // determine the point on where the slide is zoomed in
        _zoomTarget.Set((_zoomPoint.X - _pos.X) / Scale, (_zoomPoint.Y - _pos.Y) / Scale);

        // apply zoom
        _scale += zoomDelta * Factor;
        _scale = Math.Max(MinScale, Math.Min(MaxScale, _scale));

        // calculate x and y based on zoom
        _pos.Set(
            -_zoomTarget.X * Scale + _zoomPoint.X,
            -_zoomTarget.Y * Scale + _zoomPoint.Y
        );
        
        NavigationPlaneContext.TopLeftPos.Set(_pos);
        NavigationPlaneContext.Scale = Scale;

        Update();
    }

    private void OnMouseDown(MouseEventArgs e)
    {
        // Console.WriteLine("OnMouseDown NavPlane");

        _lastMousePosition.Set(e.PageX, e.PageY);
        _dragStarted = true;
        StateHasChanged();
    }

    [JSInvokable]
    public void OnMouseLeaveUp()
    {
        if (!_dragStarted) return;
        // Console.WriteLine("OnMouseLeaveUp NavPlane");
        
        _dragStarted = false;
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

        NavigationPlaneContext.TopLeftPos.Set(_pos);

        // Console.WriteLine($"Pos: X: {((_pos.X) / _scale)}, Y: {(_pos.Y) / _scale}");
        // Console.WriteLine($"Pos: X: {((_pos.X - _viewPortSize.X) / _scale)}, Y: {(_pos.Y - _viewPortSize.Y) / _scale}");
        // Console.WriteLine($"_viewPortSize: X: {_viewPortSize.X}, Y: {_viewPortSize.Y}");

        Update();
    }
    
    private float Scale => (float)Math.Exp(_scale - 4);
    
    private string GetNavigationStyle()
    {
        return ($"width: {_size.X.ToString(_nF)}px; height: {_size.Y.ToString(_nF)}px; transform:" +
                $"translate({_pos.X.ToString(_nF)}px, {_pos.Y.ToString(_nF)}px) " +
                $"scale({Scale.ToString(_nF)}); " +
                $"{(true ? "transition: transform 0s;" : "")}"); // cursor: move;
    }
}
