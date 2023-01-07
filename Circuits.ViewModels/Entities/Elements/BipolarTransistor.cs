using Circuits.ViewModels.Math;

namespace Circuits.ViewModels.Entities.Elements;

public enum BipolarTransistorType
{
    PNP,
    NPN
}

public class BipolarTransistor : Element
{
    public Vec2 P1 // Base
    {
        get => _points[0];
        init => _points[0].Set(value);
    }
    public Vec2 P2 => _points[1]; // Collector
    public Vec2 P3 => _points[2]; // Emitter

    public bool IsFlipped { get; set; } = false;
    public BipolarTransistorType BipolarType = BipolarTransistorType.NPN;

    public BipolarTransistor()
    {
        _points.Add(new Vec2());
        _points.Add(new Vec2());
        _points.Add(new Vec2());
    }

    public override void Rotate(Direction direction)
    {
        Direction = direction;
        
        var shift = direction switch
        {
            Direction.RIGHT => new Vec2(0, 1),
            Direction.LEFT => new Vec2(2, 1),
            Direction.BOTTOM => new Vec2(1, 0),
            Direction.TOP => new Vec2(1, 2),
            _ => new Vec2()
        };

        ShiftFromTopLeft.Set(shift);
        
        switch (direction)
        {
            case Direction.TOP:
                _points[1].Set(_points[0].X + 1, _points[0].Y - 2);
                _points[2].Set(_points[0].X - 1, _points[0].Y - 2);
                break; 
            case Direction.BOTTOM:
                _points[1].Set(_points[0].X + 1, _points[0].Y + 2);
                _points[2].Set(_points[0].X - 1, _points[0].Y + 2);
                break;
            case Direction.LEFT:
                _points[1].Set(_points[0].X - 2, _points[0].Y + 1);
                _points[2].Set(_points[0].X - 2, _points[0].Y - 1);
                break;
            case Direction.RIGHT:
                _points[1].Set(_points[0].X + 2, _points[0].Y + 1);
                _points[2].Set(_points[0].X + 2, _points[0].Y - 1);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    public override bool IsHorizontal(Vec2 point = null!)
    {
        var index = _points.IndexOf(point);
        
        if (index != -1)
        {
            return Direction is Direction.LEFT or Direction.RIGHT ? index == 0 : index != 0;
        }

        return false;
    }
}