using Circuits.ViewModels.Entities.Elements;

namespace Circuits.ViewModels.Entities.Structures.Properties;

public class Capacity
{
    public double Value { get; set; }
    public List<Capacitor> Capacitors { get; set; } = new();
}