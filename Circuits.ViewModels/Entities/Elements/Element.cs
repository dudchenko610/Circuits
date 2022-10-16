using Circuits.Shared.Attributes;
using Circuits.ViewModels.Math;

namespace Circuits.ViewModels.Entities.Elements;

public enum Direction
{
    [DirectionValue(Angle = 0)]
    RIGHT = 0,
    [DirectionValue(Angle = 90)]
    BOTTOM = 1,
    [DirectionValue(Angle = 180)]
    LEFT = 2,
    [DirectionValue(Angle = 270)]
    TOP = 3,
}

public abstract class Element
{
    public int Number { get; set; }
    public IReadOnlyList<Vec2> Points { get; }
    protected readonly List<Vec2> _points;

    public Direction Direction { get; protected set; }
    public Vec2 ShiftFromTopLeft { get; protected set; } = new();
    public Vec2 TopLeft 
    {
        get
        {
            _topLeft.Set(Points[0]).Add(-ShiftFromTopLeft.X, -ShiftFromTopLeft.Y);
            return _topLeft;
        }
    }
    
    private readonly Vec2 _topLeft = new();
    
    protected Element()
    {
        _points = new List<Vec2>();
        Points = _points;
    }

    public abstract void Rotate(Direction direction);

    public void Translate(Vec2 dS)
    {
        foreach (var point in _points)
        {
            point.Add(dS);
        }
    }

    public abstract bool IsHorizontal(Vec2 point = null!);
}