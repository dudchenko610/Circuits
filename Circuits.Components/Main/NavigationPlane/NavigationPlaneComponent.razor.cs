using Circuits.ViewModels.Events;
using Circuits.ViewModels.Math;
using Microsoft.AspNetCore.Components.Web;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using static System.Formats.Asn1.AsnWriter;

namespace Circuits.Components.Main.NavigationPlane;

public partial class NavigationPlaneComponent
{

    private NumberFormatInfo _nF = new NumberFormatInfo
    {
        NumberDecimalSeparator = "."
    };

    private Vec2 _size = new Vec2 { X = 1566, Y = 855 };
    private Vec2 _pos = new();
    private float _scale = 1;
    private Vec2 _zoomTarget = new Vec2();
    private Vec2 _zoomPoint = new Vec2();
    private Vec2 _lastMousePosiiton = new Vec2();
    private bool _dragStarted = false;
    private float _factor = 0.3f;
    private float _maxScale = 8f;

    private async Task OnZoomDownAsync(float scale)
    {


       // await OnZoomAsync();
    }

    private void OnZoomUp()
    {
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

        _zoomPoint.Set(container.X, container.Y);

        Console.WriteLine($"_zoomPoint {_zoomPoint.X} {_zoomPoint.Y}");


        var delta = (float) Math.Max(-1, Math.Min(1, -e.DeltaY)); // cap the delta to [-1,1] for cross browser consistency

        // determine the point on where the slide is zoomed in
        _zoomTarget.Set((_zoomPoint.X - _pos.X) / _scale, (_zoomPoint.Y - _pos.Y) / _scale);


        //Console.WriteLine($"_zoomTarget {_zoomTarget.X} {_zoomTarget.Y}");

        // apply zoom
        _scale += delta * _factor * _scale;
        _scale = Math.Max(1, Math.Min(_maxScale, _scale));

        // calculate x and y based on zoom
        _pos.Set(
            -_zoomTarget.X * _scale + _zoomPoint.X, 
            -_zoomTarget.Y * _scale + _zoomPoint.Y
        );

        Update();
    }

    private void OnMouseDown(MouseEventArgs e)
    {
        _lastMousePosiiton.Set(e.PageX, e.PageY);
        _dragStarted = true;
    }

    private void OnMouseUpOut()
    {
        _dragStarted = false;
    }

    private void OnMouseMove(ExtMouseEventArgs e)
    {
        if (_dragStarted)
        {
            var mousePosition = new Vec2(e.PageX, e.PageY);
            var change = new Vec2(
                mousePosition.X - _lastMousePosiiton.X, 
                mousePosition.Y - _lastMousePosiiton.Y
            );

            _lastMousePosiiton.Set(mousePosition);
            _pos.Add(change);

            Update();
        }
    }
}
