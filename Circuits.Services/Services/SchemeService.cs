using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;

namespace Circuits.Services.Services;

public class SchemeService : ISchemeService
{
    public IReadOnlyList<Element> Elements { get; }
    public IReadOnlyList<Branch> Branches { get; }
    public IReadOnlyDictionary<int, Node> Nodes { get; }

    public SchemeService()
    {
        Elements = new List<Element>();
        Branches = new List<Branch>();
        Nodes = new Dictionary<int, Node>();
    }
}