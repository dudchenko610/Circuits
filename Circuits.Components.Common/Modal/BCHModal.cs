using Circuits.Components.Common.Models.Modal;
using Circuits.Components.Common.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Circuits.Components.Common.Modal;

public class BCHModal : ComponentBase, IDisposable
{
    [Inject] private IPopupService PopupService { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter] public RenderFragment ChildContent { get; set; } = null!;
    [Parameter] public EventCallback OnOverlayClicked { get; set; }
    
    [Parameter] public string Width { get; set; } = "200px;";
    [Parameter] public string Height { get; set; } = "200px;";
    [Parameter] public string X { get; set; } = "";
    [Parameter] public string Y { get; set; } = "";
    [Parameter] public int ZIndex { get; set; } = 999999;
    [Parameter] public string CssClass { get; set; } = string.Empty;
    [Parameter] public bool CloseOnOverflowClicked { get; set; }
    [Parameter] public bool ShowOverlay { get; set; }
    [Parameter] public bool UpdateOnAfterRender { get; set; } = true;

    private ModalModel _modalModel = new();

    private string _prevX = string.Empty;
    private string _prevY = string.Empty;
    private int _prevZIndex = 999999;
    
    private bool _show { get; set; }
    [Parameter] public EventCallback<bool> ShowChanged { get; set; }
    [Parameter] public bool Show
    {
        get => _show;
        set
        {
            if (_show == value) return;
            _show = value;

            ResetValuesFromParameters();

            if (_show) PopupService.Open(_modalModel);
            else PopupService.Close(_modalModel);
            
            ShowChanged.InvokeAsync(value);
        }
    }

    protected override void OnInitialized()
    {
        PopupService.OnOverlayClicked += OnOverlayClickedFired;
        
        ResetValuesFromParameters();

        if (Show) PopupService.Open(_modalModel);
        else PopupService.Close(_modalModel);
    }

    protected override void OnParametersSet()
    {
        if (_prevX != X || _prevY != Y || _prevZIndex != ZIndex) Update();
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
        ResetValuesFromParameters();
        _modalModel.OnUpdate?.Invoke();
    }
    
    private void ResetValuesFromParameters()
    {
        _modalModel.Fragment = ChildContent;
        _modalModel.Height = Height;
        _modalModel.Width = Width;
        _modalModel.X = X;
        _modalModel.Y = Y;
        _modalModel.CssClass = CssClass;
        _modalModel.Overlay = ShowOverlay;
        _modalModel.ZIndex = ZIndex;

        _prevX = _modalModel.X;
        _prevY = _modalModel.Y;
    }
    
    public async Task SetPositionEfficientlyAsync(string x, string y)
    {
        await JsRuntime.InvokeVoidAsync("setLeftTopToElement", _modalModel.Id, x, y);
    }
}
