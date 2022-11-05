using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;
using Circuits.ViewModels.Helpers;
using Circuits.ViewModels.Rendering;

namespace Circuits.Services.Services;

public class HighlightService : IHighlightService
{
    public event Action? OnUpdate;
    public event Action<Element, ElementDetailsModel>? OnElementDetailsUpdate;
    private readonly HashSet<Element> _highlightedElements = new();
    private readonly HashSet<Circuit> _circuitDirections = new();

    public bool IsHighlighted(Element element)
    {
        if (_highlightedElements.Contains(element))
        {
            return true;
        }

        return false;
    }

    public bool ShouldShowDirection(Circuit circuit)
    {
        return _circuitDirections.Contains(circuit);
    }

    public void Highlight(Element element, bool show)
    {
        if (show)
        {
            _highlightedElements.Add(element);
        }
        else
        {
            _highlightedElements.Remove(element);
        }

        OnUpdate?.Invoke();
    }

    public void Highlight(List<Element> elements, bool show)
    {
        if (show)
        {
            foreach (var element in elements)
            {
                _highlightedElements.Add(element);
            }
        }
        else
        {
            foreach (var element in elements)
            {
                _highlightedElements.Remove(element);
            }
        }

        OnUpdate?.Invoke();
    }

    public void HighlightCircuitDirection(Circuit circuit, bool show)
    {
        if (show) _circuitDirections.Add(circuit);
        else _circuitDirections.Remove(circuit);

        foreach (var branch in circuit.Branches)
        {
            var elementDetailsModel = new ElementDetailsModel
            {
                Circuit = circuit,
                Branch = branch,
                CircuitBranchCoDirected = GraphHelpers.IsCoDirected(circuit, branch),
                ShowCircuitDirection = show
            };
            
            foreach (var element in branch.Elements)
            {
                OnElementDetailsUpdate?.Invoke(element, elementDetailsModel);
            }
        }
    }

    public void Clear()
    {
        _highlightedElements.Clear();
        OnUpdate?.Invoke();
        OnElementDetailsUpdate?.Invoke(null!, null!);
    }
}