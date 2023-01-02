using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Structures;
using Microsoft.AspNetCore.Components;
// ReSharper disable InconsistentNaming

namespace Circuits.Components.Main.ElementOptions;

public partial class ElementOptionsComponent
{
    private enum BranchType
    {
        RLC,
        RC,
        RL,
        R
    }
    
    [Inject] public IChartService ChartService { get; set; } = null!;
    [Inject] public ISchemeService SchemeService { get; set; } = null!;

    [Parameter] public Element SelectedElement { get; set; } = null!;
    [Parameter] public EventCallback<Element> OnRemoveElement { get; set; }
    [Parameter] public EventCallback<Element> OnRotateElement { get; set; }
    [Parameter] public EventCallback<Transistor> OnTransistorFlip { get; set; }
    [Parameter] public EventCallback OnOpenElementOptions { get; set; }
    [Parameter] public EventCallback OnCloseSelected { get; set; }

    private (BranchType, Branch) GetBranchType()
    {
        var branches = new List<Branch>();
        foreach (var graph in SchemeService.Graphs)
            branches.AddRange(graph.Circuits.SelectMany(circuits => circuits.Branches));

        var branch = branches.FirstOrDefault(x => x.Elements.Contains(SelectedElement));
        if (branch == null!) throw new Exception("Unknown Element");

        var equationSystem = SchemeService.EquationSystems
            .FirstOrDefault(x => x.Variables.Any(v => v == branch.Current ||
                                                      v == branch.CurrentDerivative ||
                                                      v == branch.CapacityVoltage ||
                                                      v == branch.CapacityVoltageFirstDerivative));
        if (equationSystem == null!) throw new Exception("Element doesn't belong to EquationSystem");
        var matVars = (List<ExpressionVariable>)equationSystem.Variables;

        var currentIndex = matVars.IndexOf(branch.Current);
        var currentDerIndex = matVars.IndexOf(branch.CurrentDerivative);
        var capacitySecondDerIndex = matVars.IndexOf(branch.CapacityVoltageSecondDerivative);
        var capacityFirstDerIndex = matVars.IndexOf(branch.CapacityVoltageFirstDerivative);

        if (capacitySecondDerIndex != -1) return (BranchType.RLC, branch);
        if (capacityFirstDerIndex != -1) return (BranchType.RC, branch);
        if (currentDerIndex != -1) return (BranchType.RL, branch);
        if (currentIndex != -1) return (BranchType.R, branch);
        
        throw new Exception("Unknown branch type");
    }
    
    private void OnShowCurrent()
    {
        var (branchType, branch) = GetBranchType();

        if (branchType == BranchType.RLC)
        {
            var totalCapacity = (float)branch.Capacity.Value;
            ChartService.Open(branch.CapacityVoltageFirstDerivative, "A", x => totalCapacity * x); // Amperes
            return;
        }

        ChartService.Open(branch.Current, "A"); // Amperes
    }

    private void OnShowVoltage()
    {
        var (branchType, branch) = GetBranchType();
        var totalCapacity = (float)(branch.Capacity == null! ? 0 : branch.Capacity.Value);

        switch (branchType)
        {
            case BranchType.RLC:
                switch (SelectedElement)
                {
                    case Resistor resistor:
                        var resistance = (float) resistor.Resistance;
                        var rc = resistance * totalCapacity; // R(ith) * C
                        
                        ChartService.Open(branch.CapacityVoltageFirstDerivative, "V", x => rc * x);
                        break;
                    case Capacitor capacitor:
                        // заряд послідовно з'єднаних конденсаторів одинаковий і дорівнює заряду всієї батареї
                        var totalCapacityDivideByIth = (float) (capacitor.Capacity / totalCapacity);
                        ChartService.Open(branch.CapacityVoltage, "V", x => totalCapacityDivideByIth * x);
                        break;
                    case Inductor inductor:
                        // (d^2Uc/dt^2) * C = dI/dt
                        var inductance = (float) inductor.Inductance;
                        var lc = inductance * totalCapacity; // L(ith) * C
                        
                        ChartService.Open(branch.CapacityVoltageSecondDerivative, "V", x => lc * x);
                        break;
                    case DCSource dcSource: // ???
                        break;
                }
                break;
            case BranchType.RC:
                switch (SelectedElement)
                {
                    case Resistor resistor:
                        var resistance = (float) resistor.Resistance;
                        ChartService.Open(branch.Current, "V", x => resistance * x);
                        break;
                    case Capacitor capacitor:
                        // заряд послідовно з'єднаних конденсаторів одинаковий і дорівнює заряду всієї батареї
                        var totalCapacityDivideByIth = (float) (capacitor.Capacity / totalCapacity);
                        ChartService.Open(branch.CapacityVoltage, "V", x => totalCapacityDivideByIth * x);
                        break;
                    case DCSource dcSource: // ???
                        break;
                }
                break;
            case BranchType.RL:
                switch (SelectedElement)
                {
                    case Resistor resistor:
                        var resistance = (float) resistor.Resistance;
                        ChartService.Open(branch.Current, "V", x => resistance * x);
                        break;
                    case Inductor inductor:
                        var inductance = (float) inductor.Inductance;
                        ChartService.Open(branch.CurrentDerivative, "V", x => inductance * x);
                        break;
                    case DCSource dcSource: // ???
                        break;
                }
                break;
            case BranchType.R:
                switch (SelectedElement)
                {
                    case Resistor resistor:
                        var resistance = (float) resistor.Resistance;
                        ChartService.Open(branch.Current, "V", x => resistance * x);
                        break;
                    case DCSource dcSource: // ???
                        break;
                }
                break;
            default:
                break;
        }
    }
}