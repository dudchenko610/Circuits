using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;

namespace Circuits.Services.Services.Interfaces;

public interface ISchemeService
{
    public IReadOnlyList<Element> Elements { get; }
    public IReadOnlyList<Branch> Branches { get; }
    public IReadOnlyDictionary<int, Node> Nodes { get; }
}