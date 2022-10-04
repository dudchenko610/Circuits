using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;

namespace Circuits.Services.Services.Interfaces;

public interface IHighlightService
{
    public event Action OnUpdate;
    bool IsHighlighted(Element element);
    void Highlight(IEnumerable<Element> elements);
    void Highlight(IEnumerable<Branch> branches);
    void Highlight(IEnumerable<Circuit> circuits);
    void Clear();
}