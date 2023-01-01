using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
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
        if (branch?.Current is not null) ChartService.Open(branch.Current, "A"); // Amperes
    }
}