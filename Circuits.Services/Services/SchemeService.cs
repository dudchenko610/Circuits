using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Structures;

namespace Circuits.Services.Services;

public class SchemeService : ISchemeService
{
    public IReadOnlyList<Element> Elements { get; }
    public IReadOnlyDictionary<int, Node> Nodes { get; }
    public IReadOnlyList<Branch> Branches { get; }
    public IReadOnlyList<Graph> Graphs { get; }
    public IReadOnlyList<EquationSystem> EquationSystems { get; }

    public Dictionary<EquationSystem, Dictionary<ExpressionVariable, List<double>>> SolverResult { get; }

    public event Action? OnUpdate;

    public SchemeService()
    {
        Elements = new List<Element>();
        Nodes = new Dictionary<int, Node>();
        Branches = new List<Branch>();
        Graphs = new List<Graph>();
        EquationSystems = new List<EquationSystem>();
        SolverResult = new Dictionary<EquationSystem, Dictionary<ExpressionVariable, List<double>>>();
    }
    
    public void Clear()
    {
        ((List<Element>)Elements).Clear();
        ((Dictionary<int, Node>)Nodes).Clear();
        ((List<Branch>)Branches).Clear();
        ((List<Graph>)Graphs).Clear();
        ((List<EquationSystem>)EquationSystems).Clear();
    }

    public void Update()
    {
        OnUpdate?.Invoke();
    }

    public void Reindex()
    {
        var nodes = (Dictionary<int, Node>)Nodes;
        nodes.Clear();

        foreach (var element in Elements)
        {
            for (var i = 0; i < element.Points.Count; i++)
            {
                var point = element.Points[i];

                var hashCode = ((int)point.X << 16) | (int)point.Y;
                var node = GetNodeData(nodes, hashCode);

                node.NodeElements.Add(new NodeElement
                {
                    PointIndex = i,
                    Element = element                                                                                                                                           
                });
            }
        }
    }
    
    private Node GetNodeData(Dictionary<int, Node> nodes, int hashCode)
    {
        if (nodes.TryGetValue(hashCode, out var node)) return node;

        node = new Node();
        nodes.Add(hashCode, node);

        return node;
    }
}