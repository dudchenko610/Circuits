using Circuits.Shared.Attributes;
using Circuits.Shared.Extensions;
using Circuits.ViewModels.Math;

namespace Circuits.ViewModels.Entities.Elements;

public class Inductor : Element
{
    public double Inductance { get; set; } = 0.01f;

    public Vec2 P1
    {
        get => _points[0];
        private set => _points[0].Set(value);
    }

    public Vec2 P2 => _points[1];

    public Inductor(Vec2 p1, Direction direction)
    {
        Direction = direction;
        
        _points.Add(new Vec2());
        _points.Add(new Vec2());

        var isHorizontal = direction.GetValue<int, DirectionValueAttribute>(x => x.Angle) % 180 == 0;
        
        P1.Set(p1);
        _points[1].Set(p1.X + (isHorizontal ? 2 : 0), p1.Y + (isHorizontal ? 0 : 2));
    }

    public override void Rotate(Direction direction)
    {
        const int length = 2;
        const int halfLength = 1;
        
        if (IsHorizontal() && direction is Direction.TOP or Direction.BOTTOM)
        {
            P1 = new Vec2(P1.X + halfLength, P1.Y - halfLength);
            _points[1].Set(P1.X, P1.Y + length);
        }
        else if(!IsHorizontal() && direction is Direction.LEFT or Direction.RIGHT)
        {
            P1 = new Vec2(P1.X - halfLength, P1.Y + halfLength);
            _points[1].Set(P1.X + length, P1.Y);
        }
        
        Direction = direction;
    }

    public override bool IsHorizontal(Vec2 _ = null!) => (int)P1.Y == (int)P2.Y;
}