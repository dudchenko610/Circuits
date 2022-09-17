using Circuits.ViewModels.Math;

namespace Circuits.ViewModels.Entities.Elements;

public class Wire : Element
{
    public Vec2 P1
    {
        get => _points[0];
        private set => _points[0].Set(value);
    }

    public Vec2 P2
    {
        get => _points[1];
        private set => _points[1].Set(value);
    }

    public Wire(Vec2 p1, Vec2 p2)
    {
        _points.Add(new Vec2());
        _points.Add(new Vec2());
        
        P1 = p1;
        P2 = p2;

        Direction = IsHorizontal() ? Direction.RIGHT : Direction.BOTTOM;
    }

    public override void Rotate(Direction direction)
    {
        if (IsHorizontal() && direction is Direction.TOP or Direction.BOTTOM)
        {
            var length = (int)(P2.X - P1.X);
            var halfLength = length / 2;
        
            P1 = new Vec2(P1.X + halfLength, P1.Y - halfLength);
            P2 = new Vec2(P1.X, P1.Y + length);
        }
        else if(!IsHorizontal() && direction is Direction.LEFT or Direction.RIGHT)
        {
            var length = (int)(P2.Y - P1.Y);
            var halfLength = length / 2;
        
            P1 = new Vec2(P1.X - halfLength, P1.Y + halfLength);
            P2 = new Vec2(P1.X + length, P1.Y);
        }
        
        Direction = direction;
    }

    public override bool IsHorizontal(Vec2 _ = null!) => (int)P1.Y == (int)P2.Y;
}