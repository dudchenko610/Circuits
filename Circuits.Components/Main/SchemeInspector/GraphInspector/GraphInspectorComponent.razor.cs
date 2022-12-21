using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeInspector.GraphInspector;

public partial class GraphInspectorComponent : IDisposable
{
    [Inject] private IElementService ElementService { get; set; } = null!;
    [Inject] private ISchemeService SchemeService { get; set; } = null!;
    [Inject] private IGraphService GraphService { get; set; } = null!;
    [Inject] private IHighlightService HighlightService { get; set; } = null!;

    private readonly List<Element> _selectedElements = new();
    private readonly List<Branch> _selectedBranches = new();
    
    protected override void OnInitialized()
    {
        ElementService.OnUpdate += StateHasChanged;
    }

    public void Dispose()
    {
        ElementService.OnUpdate -= StateHasChanged;
    }
    
    // protected override void OnAfterRender(bool firstRender)
    // {
    //     Console.WriteLine("ElementDetails OnAfterRender");
    // }

    private void OnElementSelected(Element element, bool added)
    {
        HighlightService.Highlight(element, added);
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

    private void OnBuildBranchesClicked()
    {
        GraphService.BuildBranches();
        StateHasChanged();
    }

    private void OnBuildSpanningTrees()
    {
        GraphService.BuildSpanningTrees();
    }
    
    private void OnHighlightSpanningTrees()
    {
        var removeElements = _selectedElements.ToList();
        
        _selectedElements.Clear();
        HighlightService.Highlight(removeElements, false);

        foreach (var graph in SchemeService.Graphs)
        {
            foreach (var branch in graph.SpanningTree)
            {
                foreach (var element in branch.Elements)
                {
                    if (!_selectedElements.Contains(element))
                    {
                        _selectedElements.Add(element);
                    }
                }
            
                HighlightService.Highlight(branch.Elements, true);
            }
        }
    }
    
    private void OnHighlightLeftoverBranches()
    {
        var removeElements = _selectedElements.ToList();
        
        _selectedElements.Clear();
        HighlightService.Highlight(removeElements, false);

        foreach (var graph in SchemeService.Graphs)
        {
            foreach (var branch in graph.LeftoverBranches)
            {
                foreach (var element in branch.Elements)
                {
                    if (!_selectedElements.Contains(element))
                    {
                        _selectedElements.Add(element);
                    }
                }
            
                HighlightService.Highlight(branch.Elements, true);
            }
        }
    }

    private void OnFindIndependentCircuits()
    {
        GraphService.FindFundamentalCycles();
        StateHasChanged();
    }

    private void OnSelectCircuit(Circuit circuit, bool added)
    {
        foreach (var branch in circuit.Branches)
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

    private void OnClearSelection()
    {
        HighlightService.Clear();
    }

    private void OnClearScheme()
    {
        OnClearSelection();
        SchemeService.Clear();
        SchemeService.Update();
        StateHasChanged();
    }

    private void OnPerformAllSteps()
    {
        GraphService.BuildBranches();
        GraphService.BuildSpanningTrees();
        GraphService.FindFundamentalCycles();
        GraphService.CollectProperties();
        StateHasChanged();
    }
}