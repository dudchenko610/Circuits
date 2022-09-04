using Circuits.ViewModels.Entities.Elements;

namespace Circuits.Services.Services.Interfaces;

public interface ISchemeService
{
    public IReadOnlyList<Element> Elements { get; }
    public event Action OnUpdate; 

    void Add(Element element);
    void Remove(Element element);
    void Update();
}