using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;

namespace Circuits.Services.Services;

public class SchemeService : ISchemeService
{
    public IReadOnlyList<Element> Elements { get; }
    public IReadOnlyDictionary<int, Node> Nodes { get; }
    public IReadOnlyList<Branch> Branches { get; }
    public IReadOnlyList<Branch> SpanningTree { get; }
    public IReadOnlyList<Branch> LeftoverBranches { get; }
    public IReadOnlyList<Circuit> Circuits { get; }

    public SchemeService()
    {
        Elements = new List<Element>();
        Nodes = new Dictionary<int, Node>();
        Branches = new List<Branch>();
        SpanningTree = new List<Branch>();
        LeftoverBranches = new List<Branch>();
        Circuits = new List<Circuit>();
    }
}