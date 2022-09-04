using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;

namespace Circuits.Services.Services;

public class SchemeService : ISchemeService
{
    public IReadOnlyList<Element> Elements { get; }

    public event Action? OnUpdate;
    private readonly List<Element> _elements = new();

    public SchemeService()
    {
        Elements = _elements;
    }
    
    public void Add(Element element)
    {
        _elements.Add(element);
    }

    public void Remove(Element element)
    {
        _elements.Remove(element);
    }

    public void Update()
    {
        OnUpdate?.Invoke();
    }
}