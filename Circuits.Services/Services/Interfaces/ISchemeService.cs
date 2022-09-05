using Circuits.ViewModels.Entities.Elements;

namespace Circuits.Services.Services.Interfaces;

public interface ISchemeService
{
    public IReadOnlyList<Element> Elements { get; }
    public event Action OnUpdate; 

    void Add(Element element);
    void Remove(Element element);
    bool Overlap(Element e1, Element e2);
    bool Intersects(Element element);
    void Update();
}