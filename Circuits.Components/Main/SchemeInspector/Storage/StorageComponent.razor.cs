using Circuits.Services.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeInspector.Storage;

public partial class StorageComponent
{
    [Inject] public IStorageService StorageService { get; set; } = null!;

    private async Task OnSaveClickedAsync()
    {
        await StorageService.SaveAsync();
    }
    
    private async Task OnRestoreClickedAsync()
    {
        await StorageService.RestoreAsync();
    }
}