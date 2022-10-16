using Circuits.Components.Common.Models.Modal;

namespace Circuits.Components.Common.Services.Interfaces;

public interface IPopupService
{
    event Action<ModalModel> OnOpen;
    event Action<ModalModel> OnOverlayClicked;

    void Open(ModalModel modalModel);
    void FireOverlayClicked(ModalModel modalModel);
}