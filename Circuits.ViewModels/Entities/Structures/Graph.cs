namespace Circuits.ViewModels.Entities.Structures;

public class Graph
{
    public List<Branch> SpanningTree { get; set; } = new();
    public List<Branch> LeftoverBranches { get; set; } = new();
    public List<Circuit> Circuits { get; set; } = new();
}