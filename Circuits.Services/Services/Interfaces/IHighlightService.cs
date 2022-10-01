using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;

namespace Circuits.Services.Services.Interfaces;

public interface IHighlightService
{
    public event Action OnUpdate;
    bool IsHighlighted(Element element);
    void Highlight(List<Element> elements);
    void Highlight(Branch branch, bool add);
    void Clear();
}