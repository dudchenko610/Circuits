using Circuits.ViewModels.Rendering;

namespace Circuits.Pages.Main;

public partial class MainPage : IDisposable
{
    private SchemeRendererContext _context = new();

    protected override void OnInitialized()
    {
        _context.OnUpdate += StateHasChanged;
    }

    public void Dispose()
    {
        _context.OnUpdate -= StateHasChanged;
    }
    
    private void OnPenClicked()
    {
        _context.PencilMode = !_context.PencilMode;
    }
}
