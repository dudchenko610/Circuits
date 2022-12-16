using System.Globalization;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Structures;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeInspector.EquationsInspector;

public partial class EquationsInspectorComponent : IDisposable
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
    [Inject] private IJSEquationSystemSolver JsEquationSystemSolver { get; set; } = null!;

    // private string _eliminationStatus = "Success";

    private readonly List<Element> _selectedElements = new();
    private readonly List<Branch> _selectedBranches = new();

    private List<double> _testDataArray = new();
    private bool _showTestGraph = false;

    protected override void OnInitialized()
    {
        JsEquationSystemSolver.TestReadyCallback += OnRenderTestGraph;
    }
    
    public void Dispose()
    {
        JsEquationSystemSolver.TestReadyCallback -= OnRenderTestGraph;
    }

    private void OnRenderTestGraph(List<double> testArray)
    {
        _testDataArray = testArray;
        _showTestGraph = true;
        
        StateHasChanged();
    }
    
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
            EquationSystemService.PerformGaussianElimination(equationSystem);
        }
        
        // _eliminationStatus = _equationSystemService.PerformKirchhoffElimination(_equationSystem);
        StateHasChanged();
    }

    private void OnBuildJsFunctions()
    {
        JsEquationSystemSolver.BuildJsFunctions();
    }

    private async Task TestDotNetSideSolverAsync()
    {
        await JsEquationSystemSolver.TestSolveAsync();
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