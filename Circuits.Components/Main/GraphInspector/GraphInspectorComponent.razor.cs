using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.GraphInspector;

public partial class GraphInspectorComponent : IDisposable
{
    [Inject] private IElementService _elementService { get; set; } = null!;
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
}