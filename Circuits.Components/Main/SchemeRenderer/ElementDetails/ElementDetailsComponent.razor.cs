using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;
using Circuits.ViewModels.Helpers;
using Circuits.ViewModels.Rendering;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeRenderer.ElementDetails;

public partial class ElementDetailsComponent : IDisposable
{
    private struct CircuitDirection
    {
        public Circuit Circuit { get; set; }
        public bool Reversed { get; set; }
    };
    
    [Inject] private IHighlightService HighlightService { get; set; } = null!;
    [Inject] private ISchemeService SchemeService { get; set; } = null!;
    [Parameter] public Element Element { get; set; } = null!;

    private readonly List<CircuitDirection> _circuitDirections = new();
    private bool? _showBranchDirection = null;

    protected override void OnInitialized()
    {
        HighlightService.OnElementDetailsUpdate += OnDetailsUpdate;
    }
    
    public void Dispose()
    {
        HighlightService.OnElementDetailsUpdate -= OnDetailsUpdate;
    }

    private void OnDetailsUpdate(Element element, ElementDetailsModel model)
    {
        if (element == null!) //clear
        {
            _circuitDirections.Clear();
            _showBranchDirection = null;
            StateHasChanged();
            
            return;
        }

        if (element != Element) return;

        if (model.Circuit == null!) // show / hide branch direction
        {
            _showBranchDirection = model.ShowDirection 
                ? GraphHelpers.IsCoDirected(SchemeService.Nodes, model.Branch, element)
                : null;
            
            StateHasChanged();
            return;
        }

        if (model.ShowDirection)
        {
            var elementBranchCoDirected = GraphHelpers.IsCoDirected(SchemeService.Nodes, model.Branch, element);
            var reversed = !elementBranchCoDirected;

            if (model.CircuitBranchCoDirected) reversed = !reversed;
            
            _circuitDirections.Add(new CircuitDirection
            {
                Circuit = model.Circuit,
                Reversed = reversed
            });
        }
        else
        {
            _circuitDirections.RemoveAll(x => x.Circuit == model.Circuit);
        }
        
        StateHasChanged();
    }

    // protected override void OnAfterRender(bool firstRender)
    // {
    //     Console.WriteLine("ElementDetails OnAfterRender");
    // }

    private string GetTypeClass()
    {
        return Element switch
        {
            Wire => "wire",
            Resistor => "resistor",
            Capacitor => "capacitor",
            Inductor => "inductor",
            DCSource => "dc-source",
            _ => ""
        };
    }
}