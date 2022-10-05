using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;

namespace Circuits.Services.Services;

public class HighlightService : IHighlightService
{
    public event Action? OnUpdate;
    private readonly HashSet<Element> _highlightedElements = new();
    
    public bool IsHighlighted(Element element)
    {
        if (_highlightedElements.Contains(element))
        {
            return true;
        }

        return false;
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

    public void Clear()
    {
        _highlightedElements.Clear();
        OnUpdate?.Invoke();
    }
}