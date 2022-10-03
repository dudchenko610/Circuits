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

    protected override void OnInitialized()
    {
        _elementService.OnUpdate += StateHasChanged;
    }

    public void Dispose()
    {
        _elementService.OnUpdate -= StateHasChanged;
    }

    private void OnSelectElements(List<Element> elements)
    {
        _highlightService.Highlight(elements);
    }

    private void OnSelectBranches(List<Branch> branches)
    {
        _highlightService.Highlight(branches);
    }

    private void OnBuildBranchClicked()
    {
        _graphService.BuildBranches();
        
        StateHasChanged();
    }
}