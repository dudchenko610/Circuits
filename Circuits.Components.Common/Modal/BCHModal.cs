using Circuits.Components.Common.Models.Modal;
using Circuits.Components.Common.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Common.Modal;

public class BCHModal : ComponentBase, IDisposable
{
    [Inject] private IPopupService _popupService { get; set; } = null!;

    [Parameter] public RenderFragment ChildContent { get; set; } = null!;
    [Parameter] public EventCallback OnOverlayClicked { get; set; }
    [Parameter] public string Width { get; set; } = "200px;";
    [Parameter] public string Height { get; set; } = "200px;";
    [Parameter] public string X { get; set; } = "";
    [Parameter] public string Y { get; set; } = "";
    [Parameter] public string CssClass { get; set; } = string.Empty;
    [Parameter] public bool CloseOnOverflowClicked { get; set; }

    private readonly ModalModel _modalModel = new();

    private bool _show { get; set; }
    [Parameter] public EventCallback<bool> ShowChanged { get; set; }
    [Parameter] public bool Show
    {
        get => _show;
        set
        {
            if (_show == value) return;
            _show = value;

            _modalModel.Fragment = ChildContent;
            _modalModel.Height = Height;
            _modalModel.Width = Width;
            _modalModel.X = X;
            _modalModel.Y = Y;
            _modalModel.CssClass = CssClass;

            _popupService.Open(_show ? _modalModel : null!);

            ShowChanged.InvokeAsync(value);
        }
    }

    protected override void OnInitialized()
    {
        _popupService.OnOverlayClicked += OnOverlayClickedFired;
    }

    public void Dispose()
    {
        Show = false;
        _popupService.OnOverlayClicked -= OnOverlayClickedFired;
    }

    private void OnOverlayClickedFired(ModalModel modalModel) // TODO: multilayer popup?
    {
        _ = OnOverlayClicked.InvokeAsync();

        if (!CloseOnOverflowClicked) return;
        
        Show = false;
        StateHasChanged();
    }
}
