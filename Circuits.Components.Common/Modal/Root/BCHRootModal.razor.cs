using Circuits.Components.Common.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Common.Modal.Root;

public partial class BCHRootModal : IDisposable
{
    [Inject] private IPopupService PopupService { get; set; } = null!;

    protected override void OnInitialized()
    {
        PopupService.OnUpdate += StateHasChanged;
    }

    public void Dispose()
    {
        PopupService.OnUpdate -= StateHasChanged;
    }
}
