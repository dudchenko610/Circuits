using Circuits.ViewModels.Math;

namespace Circuits.ViewModels.Entities.Elements;

public class Transistor : Element
{
    public Vec2 P1 // Base
    {
        get => _points[0];
        init
        {
            _points[0].Set(value);
            _points[1].Set(value.X - 2, value.Y + 1);
            _points[1].Set(value.X - 2, value.Y - 1);
        }
    }

    public Vec2 P2 => _points[1]; // Collector
    public Vec2 P3 => _points[1]; // Emitter

    public Transistor()
    {
        _points.Add(new Vec2());
        _points.Add(new Vec2());
        _points.Add(new Vec2());
    }

    public override bool IsHorizontal(Vec2 point = null)
    {
        return false;
    }
}