using Circuits.ViewModels.Math;

namespace Circuits.Components.Common.Models.Zoom;

public class ZoomContext
{
    public Vec2 TopLeftPos { get; } = new();
    public float Scale { get; set; } = 1.0f;
    public float ScaleLinear { get; set; } = 1.0f;
    public float AngleInRadians { get; set; } = 1.0f;
    public Action? ZoomUp { get; set; } = null!;
    public Func<float, Task>? ZoomDown { get; set; } = null!;
    public Action? OnUpdate { get; set; } = null!;
    public bool UserInteraction { get; set; } = false;
}