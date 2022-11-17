using Circuits.Components.Common.Models.Modal;
using Circuits.Components.Common.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Common.Modal.Content;

public partial class BCHModalContent : IDisposable
{
    [Inject] private IPopupService PopupService { get; set; } = null!;

    [Parameter] public ModalModel ModalModel { get; set; } = null!;

    protected override void OnInitialized()
    {
        ModalModel.OnUpdate += StateHasChanged;
    }
    
    public void Dispose()
    {
        ModalModel.OnUpdate -= StateHasChanged;
    }
    
    private void OnOverlayClicked()
    {
        PopupService.FireOverlayClicked(ModalModel);
    }
    
    private bool IsCenter()
    {
        return string.IsNullOrWhiteSpace(ModalModel.X) || string.IsNullOrWhiteSpace(ModalModel.Y);
    }
}