using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;

namespace Circuits.Services.Services.Interfaces;

public interface ISchemeService
{
    public IReadOnlyList<Element> Elements { get; }
    public IReadOnlyDictionary<int, Node> Nodes { get; }
    public IReadOnlyList<Branch> Branches { get; }
    public IReadOnlyList<Branch> SpanningTree { get; }
    public IReadOnlyList<Branch> LeftoverBranches { get; }
    public IReadOnlyList<Circuit> Circuits { get; }
}