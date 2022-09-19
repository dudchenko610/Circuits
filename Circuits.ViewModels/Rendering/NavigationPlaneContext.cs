using Circuits.ViewModels.Math;

namespace Circuits.ViewModels.Rendering;

public class NavigationPlaneContext
{
    public Vec2 TopLeftPos { get; } = new();
    public float Scale { get; set; } = 1.0f;
}