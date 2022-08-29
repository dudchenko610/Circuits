using Circuits.ViewModels.Events;
using Circuits.ViewModels.Math;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using static System.Formats.Asn1.AsnWriter;

namespace Circuits.Components.Main.NavigationPlane;

public partial class NavigationPlaneComponent
{
    private bool _isDown = false;
    private float _scaleFactor = 0.0f;
    private float _scaleAccumulator = 1.0f;

    private Vec2 _vec2 = new();
    private Vec2 _savedZoomPos = new();

    private NumberFormatInfo _nF = new NumberFormatInfo
    {
        NumberDecimalSeparator = "."
    };

    private async Task OnZoomDownAsync(float scale)
    {
        _scaleFactor = scale;
        _isDown = true;

       // await OnZoomAsync();
    }

    private void OnZoomUp()
    {
        _isDown = false;
    }

    private void OnScroll(ScrollEventArgs e)
    {
        //_vec2 = new Vec2(
        //    e.ScrollLeft + e.ClientWidth * 0.5f, 
        //    e.ScrollTop + e.ClientHeight * 0.5f
        //);
    }

    //private async Task OnZoomAsync()
    //{
    //    _savedZoomPos.Set(_vec2);
    //    _scaleAccumulator += _scaleFactor;

    //    if (_scaleAccumulator < 1.0f)
    //    {
    //        _scaleAccumulator = 1.0f;
    //    }

    //    if (_scaleAccumulator > 3.5f)
    //    {
    //        _scaleAccumulator = 3.5f;
    //    }

    //    StateHasChanged();
    //    await Task.Delay(20);

    //    if (_isDown)
    //    {
    //        await OnZoomAsync();
    //    }
    //}

    private Vec2 _prevOrigin = new();
    private Vec2 _nextOrigin = new();
    private float _zoomIntensity = 0.005f;
    private float _zoom = 1;
    private float _scale = 1;

    private void OnMouseWheel(ExtWheelEventArgs e)
    {
        //_savedZoomPos.Set(_vec2.X, _vec2.Y);

        //_scaleFactor = (float)-e.DeltaY * 0.001f;
        //_scaleAccumulator += _scaleFactor;

        //if (_scaleAccumulator < 1.0f)
        //{
        //    _scaleAccumulator = 1.0f;
        //}

        //if (_scaleAccumulator > 3.5f)
        //{
        //    _scaleAccumulator = 3.5f;
        //}

        var scroll = e.DeltaY < 0 ? 1 : -2;
        _zoom = (float) Math.Exp(scroll * _zoomIntensity);

        _prevOrigin.Set(_nextOrigin);
        _nextOrigin.Add(-(e.X / (_scale * _zoom) - e.X / _scale), -(e.Y / (_scale * _zoom) - e.Y / _scale));

        // Updating scale and visisble width and height
        _scale *= _zoom;

        StateHasChanged();
    }

    private void OnMouseMove(ExtMouseEventArgs e)
    {
        var navigationPanel = e.PathCoordinates.FirstOrDefault(x => x.ClassList.Contains("navigation-scroller-panel"));

        if (navigationPanel == null)
            return;

        //_vec2.Set(navigationPanel.X, navigationPanel.Y);
        //_vec2.Multiply(1.0f / _scaleAccumulator);

        //Console.WriteLine($"move x: {_vec2.X} y: {_vec2.Y} scale: {_scaleAccumulator}");
    }
}
