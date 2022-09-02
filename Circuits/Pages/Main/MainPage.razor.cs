using Circuits.ViewModels.Rendering;

namespace Circuits.Pages.Main;

public partial class MainPage
{
    private SchemeRendererContext _context = new();

    private void OnPenClicked()
    {
        _context.PencilMode = true;
        StateHasChanged();
    }
}
