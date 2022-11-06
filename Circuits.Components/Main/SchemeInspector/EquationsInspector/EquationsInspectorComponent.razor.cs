using System.Globalization;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Structures;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeInspector.EquationsInspector;

public partial class EquationsInspectorComponent
{
    [Inject] private IEquationSystemService _equationSystemService { get; set; } = null!;
    [Inject] private IElectricalSystemService _electricalSystemService { get; set; } = null!;
    [Inject] private ISchemeService _schemeService { get; set; } = null!;
    [Inject] private IHighlightService _highlightService { get; set; } = null!;

    // private string _eliminationStatus = "Success";

    private readonly List<Element> _selectedElements = new();
    private readonly List<Branch> _selectedBranches = new();
    
    private void OnBuildEquationSystems()
    {
        _electricalSystemService.BuildEquationSystemsFromGraphs(_schemeService.Graphs);
        StateHasChanged();
    }
    
    private void OnSelectCircuit(Circuit circuit, bool added)
    {
        foreach (var branch in circuit.Branches)
        {
            _highlightService.Highlight(branch.Elements, added);

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
    }
    
    private void OnBranchSelected(Branch branch, bool added)
    {
        _highlightService.Highlight(branch.Elements, added);
        
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
        var action = !_highlightService.ShouldShowDirection(circuit);
        _highlightService.HighlightCircuitDirection(circuit, action);
        StateHasChanged();
    }
    
    private void OnShowBranchDirectionClicked(Branch branch)
    {
        var action = !_highlightService.ShouldShowDirection(branch);
        _highlightService.HighlightBranchDirection(branch, action);
        StateHasChanged();
    }
    
    private void OnPerformGaussianElimination()
    {
        foreach (var equationSystem in _schemeService.EquationSystems)
        {
            _equationSystemService.PerformGaussianElimination(equationSystem);
        }
        
        // _eliminationStatus = _equationSystemService.PerformKirchhoffElimination(_equationSystem);
        StateHasChanged();
    }

    private void OnClear()
    {
        // _eliminationStatus = "Success";

        _highlightService.Clear();
        _schemeService.Clear();
        _schemeService.Update();
        
        StateHasChanged();
    }
}