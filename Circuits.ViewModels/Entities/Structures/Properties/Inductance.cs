using Circuits.ViewModels.Entities.Elements;

namespace Circuits.ViewModels.Entities.Structures.Properties;

public class Inductance
{
    public double Value { get; set; }
    public List<Inductor> Inductors { get; set; } = new();
}