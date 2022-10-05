using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;

namespace Circuits.Services.Services;

public class SchemeService : ISchemeService
{
    public IReadOnlyList<Element> Elements { get; }
    public IReadOnlyDictionary<int, Node> Nodes { get; }
    public IReadOnlyList<Branch> Branches { get; }
    public IReadOnlyList<Graph> Graphs { get; }

    public SchemeService()
    {
        Elements = new List<Element>();
        Nodes = new Dictionary<int, Node>();
        Branches = new List<Branch>();
        Graphs = new List<Graph>();
    }
}