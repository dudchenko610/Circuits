using Circuits.Components.Common.Models.Modal;
using Circuits.Components.Common.Services.Interfaces;

namespace Circuits.Components.Common.Services;

public class PopupService : IPopupService
{
    public event Action<ModalModel> OnOpen = null!;
    public event Action<ModalModel> OnOverlayClicked = null!;

    public void Open(ModalModel modalModel)
    {
        OnOpen?.Invoke(modalModel);
    }

    public void FireOverlayClicked(ModalModel modalModel)
    {
        OnOverlayClicked?.Invoke(modalModel);
    }
}