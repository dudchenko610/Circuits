using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;

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

    public void Highlight(List<Element> elements)
    {
        if (elements == null!) return;
        
        _highlightedElements.Clear();

        foreach (var element in elements)
        {
            _highlightedElements.Add(element);
        }
        
        OnUpdate?.Invoke();
    }

    public void Highlight(Branch branch, bool add)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        _highlightedElements.Clear();
        OnUpdate?.Invoke();
    }
}