using Circuits.ViewModels.Entities.Elements;

namespace Circuits.ViewModels.Entities.Structures;

public class NodeElement
{
    public Element Element { get; set; } = null!;
    public int PointIndex { get; set; }
}