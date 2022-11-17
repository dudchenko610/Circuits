using Circuits.Components.Common.Models.Modal;
using Circuits.Components.Common.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Common.Modal;


public class BCHModal : ComponentBase, IDisposable
{
    [Inject] private IPopupService PopupService { get; set; } = null!;

    [Parameter] public RenderFragment ChildContent { get; set; } = null!;
    [Parameter] public EventCallback OnOverlayClicked { get; set; }
    
    [Parameter] public string Width { get; set; } = "200px;";
    [Parameter] public string Height { get; set; } = "200px;";
    [Parameter] public string X { get; set; } = "";
    [Parameter] public string Y { get; set; } = "";
    [Parameter] public string CssClass { get; set; } = string.Empty;
    [Parameter] public bool CloseOnOverflowClicked { get; set; }
    [Parameter] public bool ShowOverlay { get; set; }
    [Parameter] public bool UpdateOnAfterRender { get; set; } = true;

    private ModalModel _modalModel = new();

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
            _modalModel.Overlay = ShowOverlay;

            if (_show) PopupService.Open(_modalModel);
            else PopupService.Close(_modalModel);
            
            ShowChanged.InvokeAsync(value);
        }
    }

    protected override void OnInitialized()
    {
        PopupService.OnOverlayClicked += OnOverlayClickedFired;
        
        _modalModel.Fragment = ChildContent;
        _modalModel.Height = Height;
        _modalModel.Width = Width;
        _modalModel.X = X;
        _modalModel.Y = Y;
        _modalModel.CssClass = CssClass;
        _modalModel.Overlay = ShowOverlay;

        if (Show) PopupService.Open(_modalModel);
        else PopupService.Close(_modalModel);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (UpdateOnAfterRender) _modalModel.OnUpdate?.Invoke();
    }
    
    public void Dispose()
    {
        PopupService.Close(_modalModel);
        PopupService.OnOverlayClicked -= OnOverlayClickedFired;
    }

    private void OnOverlayClickedFired(ModalModel modalModel)
    {
        OnOverlayClicked.InvokeAsync();

        if (!CloseOnOverflowClicked) return;
        
        Show = false;
        StateHasChanged();
    }

    public void Update()
    {
        _modalModel.OnUpdate?.Invoke();
    }
}
