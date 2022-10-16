using Circuits.Components.Common.Models.Modal;
using Circuits.Components.Common.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Common.Modal.Root;

public partial class BCHRootModal : IDisposable
{
    [Inject] private IPopupService _popupService { get; set; } = null!;

    private ModalModel _modalModel = null!;

    protected override void OnInitialized()
    {
        _popupService.OnOpen += OnOpen;
    }

    public void Dispose()
    {
        _popupService.OnOpen -= OnOpen;
    }

    private void OnOpen(ModalModel modalModel)
    {
        _modalModel = modalModel;
        StateHasChanged();
    }

    private void OnOverlayClicked()
    {
        _popupService.FireOverlayClicked(_modalModel);
    }

    private bool IsCenter()
    {
        return string.IsNullOrWhiteSpace(_modalModel.X) || string.IsNullOrWhiteSpace(_modalModel.Y);
    }
}
