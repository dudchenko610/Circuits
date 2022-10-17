using Circuits.ViewModels.Entities.Elements;

namespace Circuits.ViewModels.Entities.Structures.Properties;

public class Resistance
{
    public double Value { get; set; }
    public List<Resistor> Resistors { get; set; } = new();
}