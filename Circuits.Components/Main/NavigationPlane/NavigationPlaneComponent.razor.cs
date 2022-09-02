using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Events;
using Circuits.ViewModels.Math;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Xml.Linq;
using Circuits.ViewModels.Rendering;
using static System.Formats.Asn1.AsnWriter;

namespace Circuits.Components.Main.NavigationPlane;

public partial class NavigationPlaneComponent : IDisposable
{
    [Inject] private IJSUtilsService _jsUtilsService { get; set; } = null!;
    [Parameter] public RenderFragment<NavigationPlaneContext> ContentTemplate { get; set; } = null!;
    [Parameter] public RenderFragment ControlTemplate { get; set; } = null!;

    private string _navigationId = $"_id_{Guid.NewGuid()}";
    private NumberFormatInfo _nF = new () { NumberDecimalSeparator = "." };

    private Vec2 _size = new () { X = 100, Y = 100 };
    private Vec2 _pos = new();
    private float _scale = 1;
    private Vec2 _zoomTarget = new();
    private Vec2 _zoomPoint = new();
    private Vec2 _lastMousePosition = new();
    private bool _dragStarted = false;
    private float _factor = 0.3f;
    private float _maxScale = 12f;
    private bool _zoomKeep = false;

    private NavigationPlaneContext _navigationPlaneContext = new();
    
    protected override void OnInitialized()
    {
        IJSUtilsService.OnResize += OnResizeAsync;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await OnResizeAsync();
        }
    }

    public void Dispose()
    {
        IJSUtilsService.OnResize -= OnResizeAsync;
    }

    private async Task OnResizeAsync()
    {
        var navigationRect = await _jsUtilsService.GetBoundingClientRectAsync(_navigationId);
        _size.Set(navigationRect.Width, navigationRect.Height);
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
            OnZoom(_size.X * 0.5f, _size.Y * 0.5f, zoomDelta);
            await Task.Delay(100);
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

        if (_pos.X + _size.X * _scale < _size.X)
        {
            _pos.X = -_size.X * (_scale - 1);
        }

        if (_pos.Y > 0)
        {
            _pos.Y = 0;
        }

        if (_pos.Y + _size.Y * _scale < _size.Y)
        {
            _pos.Y = -_size.Y * (_scale - 1);
        }

        StateHasChanged();
    }

    private void OnMouseWheel(ExtWheelEventArgs e)
    {
        var container = e.PathCoordinates.FirstOrDefault(x => x.ClassList.Contains("navigation-cs-container"));

        if (container == null)
            return;

        OnZoom(container.X, container.Y, - (float)e.DeltaY);
    }

    private void OnZoom(float x, float y, float controlDelta)
    {
        _zoomPoint.Set(x, y);
        var delta = (float)Math.Max(-1, Math.Min(1, controlDelta)); // cap the delta to [-1,1] for cross browser consistency

        // determine the point on where the slide is zoomed in
        _zoomTarget.Set((_zoomPoint.X - _pos.X) / _scale, (_zoomPoint.Y - _pos.Y) / _scale);

        // apply zoom
        _scale += delta * _factor * _scale;
        _scale = Math.Max(1, Math.Min(_maxScale, _scale));

        // calculate x and y based on zoom
        _pos.Set(
            -_zoomTarget.X * _scale + _zoomPoint.X,
            -_zoomTarget.Y * _scale + _zoomPoint.Y
        );

        _navigationPlaneContext.Scale = _scale;

        Update();
    }

    private void OnMouseDown(MouseEventArgs e)
    {
        _lastMousePosition.Set(e.PageX, e.PageY);
        _dragStarted = true;
    }

    private void OnMouseLeaveUp()
    {
        _dragStarted = false;
    }

    private void OnMouseMove(ExtMouseEventArgs e)
    {
        if (_dragStarted)
        {
            var mousePosition = new Vec2(e.PageX, e.PageY);
            var change = new Vec2(
                mousePosition.X - _lastMousePosition.X, 
                mousePosition.Y - _lastMousePosition.Y
            );

            _lastMousePosition.Set(mousePosition);
            _pos.Add(change);

            Update();
        }
    }
}
