using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;

namespace Circuits.Services.Services.Interfaces;

public interface IHighlightService
{
    public event Action OnUpdate;
    bool IsHighlighted(Element element);
    void Highlight(Element element, bool show);
    void Highlight(List<Element> elements, bool show);
    void Clear();
}