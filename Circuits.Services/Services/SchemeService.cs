using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;

namespace Circuits.Services.Services;

public class SchemeService : ISchemeService
{
    class NodeData
    {
        public Element Element { get; set; }
        public int PointIndex { get; set; }
    }
    
    public IReadOnlyList<Element> Elements { get; }

    public event Action? OnUpdate;
    private readonly List<Element> _elements = new();

    private Dictionary<int, List<NodeData>> _nodesHashMap = new();

    public SchemeService()
    {
        Elements = _elements;
    }
    
    public void Add(Element element)
    {
        _elements.Add(element);

        foreach (var point in element.Points)
        {
            var hashCode = ((int) point.X << 16) | (int) point.Y;
            var nodeData = GetNodeData(hashCode);

        }
    }

    public void Remove(Element element)
    {
        _elements.Remove(element);
    }

    public bool Overlap(Element e1, Element e2)
    {
        
        return true;
    }

    public void Update()
    {
        OnUpdate?.Invoke();
    }

    public bool Intersects(Element element)
    {
        foreach (var el in _elements)
        {
            if (el != element && Intersects(el, element))
                return true;
        }
        
        return false;
    }

    private List<NodeData> GetNodeData(int hashCode)
    {
        if (_nodesHashMap.TryGetValue(hashCode, out var nodeDataList)) return nodeDataList;
        
        nodeDataList = new List<NodeData>();
        _nodesHashMap.Add(hashCode, nodeDataList);

        return nodeDataList;
    }

    private bool Intersects(Element e1, Element e2)
    {
        if (e1 is Wire w1 && e2 is Wire w2)
        {
            var w1IsHorizontal = (int) w1.P1.Y == (int) w1.P2.Y;
            var w2IsHorizontal = (int) w2.P1.Y == (int) w2.P2.Y;

            var areHorizontalInOneLine = w1IsHorizontal && w2IsHorizontal && (int)w1.P1.Y == (int)w2.P1.Y;
            var areVerticalInOneLine = !w1IsHorizontal && !w2IsHorizontal && (int)w1.P1.X == (int)w2.P1.X;
        
            return (areHorizontalInOneLine && !((w1.P2.X <= w2.P1.X) || (w2.P2.X <= w1.P1.X))) || 
                   (areVerticalInOneLine   && !((w1.P2.Y <= w2.P1.Y) || (w2.P2.Y <= w1.P1.Y)));
        }

        return false;
    }
}