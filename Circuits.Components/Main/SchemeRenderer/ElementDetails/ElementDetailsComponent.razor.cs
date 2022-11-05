using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;
using Circuits.ViewModels.Helpers;
using Circuits.ViewModels.Rendering;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeRenderer.ElementDetails;

public partial class ElementDetailsComponent : IDisposable
{
    struct CircuitDirection
    {
        public Circuit Circuit { get; set; }
        public bool Reversed { get; set; }
    };
    
    [Inject] private IHighlightService _highlightService { get; set; } = null!;
    [Inject] private ISchemeService _schemeService { get; set; } = null!;
    [Parameter] public Element Element { get; set; } = null!;

    private readonly List<CircuitDirection> _circuitDirections = new();
    
    protected override void OnInitialized()
    {
        _highlightService.OnElementDetailsUpdate += OnDetailsUpdate;
    }
    
    public void Dispose()
    {
        _highlightService.OnElementDetailsUpdate -= OnDetailsUpdate;
    }

    private void OnDetailsUpdate(Element element, ElementDetailsModel model)
    {
        if (element == null!) //clear
        {
            _circuitDirections.Clear();
            StateHasChanged();
            
            return;
        }

        if (element != Element) return;

        if (model.ShowCircuitDirection)
        {
            var elementBranchCoDirected = GraphHelpers.IsCoDirected(_schemeService.Nodes, model.Branch, element);
            var reversed = !(elementBranchCoDirected);

            if (!model.CircuitBranchCoDirected)
            {
                reversed = !reversed;
            }
            
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