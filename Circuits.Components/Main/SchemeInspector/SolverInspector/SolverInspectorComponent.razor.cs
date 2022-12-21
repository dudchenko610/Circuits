using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Structures;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeInspector.SolverInspector;

public partial class SolverInspectorComponent
{
    private class CircuitBranch
    {
        public Circuit Circuit { get; set; } = null!;
        public Branch Branch { get; set; } = null!;
    }
    
    [Inject] private IEquationSystemService EquationSystemService { get; set; } = null!;
    [Inject] private IElectricalSystemService ElectricalSystemService { get; set; } = null!;
    [Inject] private ISchemeService SchemeService { get; set; } = null!;
    [Inject] private IHighlightService HighlightService { get; set; } = null!;
    [Inject] private ISolverService SolverService { get; set; } = null!;
}