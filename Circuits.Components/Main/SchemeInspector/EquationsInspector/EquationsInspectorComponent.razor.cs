using System.Globalization;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Structures;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeInspector.EquationsInspector;

public partial class EquationsInspectorComponent
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

    // private string _eliminationStatus = "Success";

    private readonly List<Element> _selectedElements = new();
    private readonly List<Branch> _selectedBranches = new();

    private List<double> _testDataArray = new();
    
    private void OnBuildEquationSystems()
    {
        ElectricalSystemService.BuildEquationSystemsFromGraphs(SchemeService.Graphs);
        StateHasChanged();
    }

    private void OnBranchSelected(Branch branch, bool added)
    {
        HighlightService.Highlight(branch.Elements, added);
        
        foreach (var element in branch.Elements)
        {
            if (added)
            {
                if (!_selectedElements.Contains(element))
                {
                    _selectedElements.Add(element);
                }
            }
            else
            {
                _selectedElements.Remove(element);
            }  
        }
    }
    
    private void OnShowCircuitDirectionClicked(Circuit circuit)
    {
        var action = !HighlightService.ShouldShowDirection(circuit);
        HighlightService.HighlightCircuitDirection(circuit, action);
        StateHasChanged();
    }
    
    private void OnShowBranchDirectionClicked(Branch branch)
    {
        var action = !HighlightService.ShouldShowDirection(branch);
        HighlightService.HighlightBranchDirection(branch, action);
        StateHasChanged();
    }
    
    private void OnPerformGaussianElimination()
    {
        foreach (var equationSystem in SchemeService.EquationSystems)
        {
            var result = EquationSystemService.PerformGaussianElimination(equationSystem);
            Console.WriteLine($"GaussianElimination, {result}");
        }
        
        // _eliminationStatus = _equationSystemService.PerformKirchhoffElimination(_equationSystem);
        StateHasChanged();
    }

    private async Task TestBroydensMethodAsync()
    {
        // foreach (var equationSystem in SchemeService.EquationSystems)
        // {
        //     await SolverService.TestBroydensMethodAsync(equationSystem);
        // }
        
        StateHasChanged();
    }

    private void OnClear()
    {
        // _eliminationStatus = "Success";

        HighlightService.Clear();
        SchemeService.Clear();
        SchemeService.Update();
        
        StateHasChanged();
    }
}