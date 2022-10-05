using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.GraphInspector;

public partial class GraphInspectorComponent : IDisposable
{
    [Inject] private IElementService _elementService { get; set; } = null!;
    [Inject] private ISchemeService _schemeService { get; set; } = null!;
    [Inject] private IGraphService _graphService { get; set; } = null!;
    [Inject] private IHighlightService _highlightService { get; set; } = null!;

    private List<Element> _selectedElements = new();
    private List<Branch> _selectedBranches = new();
    private List<Circuit> _selectedCircuits = new();
    
    protected override void OnInitialized()
    {
        _elementService.OnUpdate += StateHasChanged;
    }

    public void Dispose()
    {
        _elementService.OnUpdate -= StateHasChanged;
    }

    private void OnElementSelected(Element element, bool added)
    {
        _highlightService.Highlight(element, added);
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

    private void OnBuildBranchesClicked()
    {
        _graphService.BuildBranches();
        StateHasChanged();
    }

    private void OnBuildSpanningTree()
    {
        _graphService.BuildSpanningTree();
    }
    
    private void OnHighlightSpanningTree()
    {
        var removeElements = _selectedElements.ToList();
        
        _selectedElements.Clear();

        _highlightService.Highlight(removeElements, false);
        
        foreach (var branch in _schemeService.SpanningTree)
        {
            foreach (var element in branch.Elements)
            {
                if (!_selectedElements.Contains(element))
                {
                    _selectedElements.Add(element);
                }
            }
            
            _highlightService.Highlight(branch.Elements, true);
        }
    }
    
    private void OnHighlightLeftoverBranches()
    {
        var removeElements = _selectedElements.ToList();
        
        _selectedElements.Clear();

        _highlightService.Highlight(removeElements, false);
        
        foreach (var branch in _schemeService.LeftoverBranches)
        {
            foreach (var element in branch.Elements)
            {
                if (!_selectedElements.Contains(element))
                {
                    _selectedElements.Add(element);
                }
            }
            
            _highlightService.Highlight(branch.Elements, true);
        }
    }

    private void OnFindIndependentCircuits()
    {
        _graphService.FindFundamentalCycles();
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
}