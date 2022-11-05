using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;
using Circuits.ViewModels.Rendering;

namespace Circuits.Services.Services.Interfaces;

public interface IHighlightService
{
    public event Action OnUpdate;
    public event Action<Element, ElementDetailsModel> OnElementDetailsUpdate;
    bool IsHighlighted(Element element);
    bool ShouldShowDirection(Circuit circuit);
    bool ShouldShowDirection(Branch branch);
    void Highlight(Element element, bool show);
    void Highlight(List<Element> elements, bool show);
    void HighlightCircuitDirection(Circuit circuit, bool show);
    void HighlightBranchDirection(Branch branch, bool show);
    void Clear();
}