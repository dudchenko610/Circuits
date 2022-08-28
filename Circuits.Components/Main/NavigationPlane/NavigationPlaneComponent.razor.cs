using Circuits.ViewModels.Events;
using Circuits.ViewModels.Math;
using System.Globalization;

namespace Circuits.Components.Main.NavigationPlane;

public partial class NavigationPlaneComponent
{
    private bool _isDown = false;
    private float _scaleFactor = 0.0f;
    private float _scaleAccumulator = 1.0f;

    private Vec2 _vec2 = new();

    private NumberFormatInfo _numberFormatWithDot = new NumberFormatInfo
    {
        NumberDecimalSeparator = "."
    };

    private async Task OnZoomDownAsync(float scale)
    {
        _scaleFactor = scale;
        _isDown = true;

        await OnZoomAsync();
    }

    private void OnZoomUp()
    {
        _isDown = false;
    }

    private void OnScroll(ScrollEventArgs e)
    {
        _vec2 = new Vec2(
            e.ScrollLeft + e.ClientWidth * 0.5f, 
            e.ScrollTop + e.ClientHeight * 0.5f
        );
    }

    private async Task OnZoomAsync()
    {
        _scaleAccumulator += _scaleFactor;

        if (_scaleAccumulator < 1.0f)
        {
            _scaleAccumulator = 1.0f;
        }

        if (_scaleAccumulator > 3.5f)
        {
            _scaleAccumulator = 3.5f;
        }

        StateHasChanged();
        await Task.Delay(20);

        if (_isDown)
        {
            await OnZoomAsync();
        }
    }


}
