using Circuits.ViewModels.Entities.Structures;

namespace Circuits.ViewModels.Rendering;

public class ElementDetailsModel
{
    public Circuit Circuit { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public bool CircuitBranchCoDirected { get; set; }
    public bool ShowCircuitDirection { get; set; }
}