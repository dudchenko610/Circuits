using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Structures;

namespace Circuits.Services.Services.Interfaces;

public interface ISchemeService
{
    IReadOnlyList<Element> Elements { get; }
    IReadOnlyDictionary<int, Node> Nodes { get; }
    IReadOnlyList<Branch> Branches { get; }
    IReadOnlyList<Graph> Graphs { get; }
    IReadOnlyList<EquationSystem> EquationSystems { get; }
    // Dictionary<EquationSystem, Dictionary<ExpressionVariable, List<double>>> SolverResult { get; }

    event Action OnUpdate; // TODO: remove from here, or maybe not
    void Clear();
    void Update();
    void Reindex();
}