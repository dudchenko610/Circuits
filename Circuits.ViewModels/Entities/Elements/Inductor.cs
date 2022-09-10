using Circuits.ViewModels.Math;

namespace Circuits.ViewModels.Entities.Elements;

public class Inductor : Element
{
    public Vec2 P1
    {
        get => _points[0];
        init
        {
            var isRight = Direction == Direction.RIGHT;
            
            _points[1].Set(value.X + (isRight ? 2 : 0), value.Y + (isRight ? 0 : 2));
            _points[0].Set(value);
        }
    }

    public Vec2 P2 => _points[1];

    public Direction Direction { get; init; } = Direction.RIGHT;

    public Inductor()
    {
        _points.Add(new Vec2());
        _points.Add(new Vec2());
    }

    public override bool IsHorizontal(Vec2 _ = null!) => (int)P1.Y == (int)P2.Y;
}