using Circuits.ViewModels.Math;

namespace Circuits.ViewModels.Entities.Elements;

public abstract class Element
{
    public IReadOnlyList<Vec2> Points { get; }
    protected readonly List<Vec2> _points;

    protected Element()
    {
        _points = new List<Vec2>();
        Points = _points;
    }

    public void Translate(Vec2 dS)
    {
        foreach (var point in _points)
        {
            point.Add(dS);
        }
    }

    public abstract bool IsHorizontal(Vec2 point = null!);
}