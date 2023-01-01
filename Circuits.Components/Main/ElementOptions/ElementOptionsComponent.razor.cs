using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Structures;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.ElementOptions;

public partial class ElementOptionsComponent
{
    [Inject] public IChartService ChartService { get; set; } = null!;
    [Inject] public ISchemeService SchemeService { get; set; } = null!;

    [Parameter] public Element SelectedElement { get; set; } = null!;
    [Parameter] public EventCallback<Element> OnRemoveElement { get; set; }
    [Parameter] public EventCallback<Element> OnRotateElement { get; set; }
    [Parameter] public EventCallback<Transistor> OnTransistorFlip { get; set; }
    [Parameter] public EventCallback OnOpenElementOptions { get; set; }
    [Parameter] public EventCallback OnCloseSelected { get; set; }

    private void OnShowCurrent()
    {
        var branches = new List<Branch>();
        foreach (var graph in SchemeService.Graphs) branches.AddRange(graph.Circuits.SelectMany(circuits => circuits.Branches));

        var branch = branches.FirstOrDefault(x => x.Elements.Contains(SelectedElement));
        if (branch == null!) return;

        var equationSystem = SchemeService.EquationSystems
            .FirstOrDefault(x => x.Variables.Any(v => v == branch.Current ||
                                             v == branch.CurrentDerivative ||
                                             v == branch.CapacityVoltage ||
                                             v == branch.CapacityVoltageFirstDerivative));
        if (equationSystem == null!) return;
        var matVars = (List<ExpressionVariable>) equationSystem.Variables;
        var capacitySecondDerIndex = matVars.IndexOf(branch.CapacityVoltageSecondDerivative);

        if (capacitySecondDerIndex != -1) // RLC case
        {
            var branchCapacity = (float) branch.Capacity.Value;
            ChartService.Open(branch.CapacityVoltageFirstDerivative, "A", x => branchCapacity * x); // Amperes
            return;
        }
        
        ChartService.Open(branch.Current, "A"); // Amperes
    }
}