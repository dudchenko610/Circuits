using Circuits.ViewModels.Math;

namespace Circuits.ViewModels.Entities.Elements;

public class Wire : Element
{
    public Vec2 P1
    {
        get => _points[0];
        set => _points[0].Set(value);
    }

    public Vec2 P2
    {
        get => _points[1];
        set => _points[1].Set(value);
    }

    public Wire()
    {
        _points.Add(new Vec2());
        _points.Add(new Vec2());
    }
}