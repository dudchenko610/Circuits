using Circuits.ViewModels.Math;

namespace Circuits.ViewModels.Entities.Elements;

public class Element
{
    public IReadOnlyList<Vec2> Points { get; }
    protected List<Vec2> _points;

    public Element()
    {
        _points = new();
        Points = _points;
    }

    public void Translate(Vec2 dS)
    {
        foreach (var point in _points)
        {
            point.Add(dS);
        }
    }
}