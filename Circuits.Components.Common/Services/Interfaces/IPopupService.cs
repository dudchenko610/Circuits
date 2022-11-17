using Circuits.Components.Common.Models.Modal;

namespace Circuits.Components.Common.Services.Interfaces;

public interface IPopupService
{
    IReadOnlyList<ModalModel> Modals { get; }

    event Action? OnUpdate;
    event Action<ModalModel>? OnOverlayClicked;

    void Open(ModalModel modalModel);
    void Close(ModalModel modalModel);
    void FireOverlayClicked(ModalModel modalModel);
}