using Circuits.ViewModels.Entities.Elements;

namespace Circuits.ViewModels.Entities.Structures;

public class Branch
{
    public List<Element> Elements { get; set; } = new();
    public Node NodeLeft { get; set; } = null!;
    public Node NodeRight { get; set; } = null!;
}