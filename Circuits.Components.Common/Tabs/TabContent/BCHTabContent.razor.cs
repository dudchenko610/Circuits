using System.Globalization;
using Circuits.Components.Common.Models.Tabs;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Markup;
using Circuits.ViewModels.Math;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Circuits.Components.Common.Tabs.TabContent;

public partial class BCHTabContent : ComponentBase, IDisposable
{
    [Inject] private IJSUtilsService _jSUtilsService { get; set; } = null!;
    [Inject] private IJSRuntime _jsRuntime { get; set; } = null!;

    [Parameter] public TabContextModel TabContext { get; set; } = null!;
    [Parameter] public int PanelId { get; set; } = -1;
    [Parameter] public RenderFragment<TabModel> Template { get; set; } = null!;

    private IJSInProcessRuntime _jsInProcessRuntime = null!;

    private TabPanelModel _panelModel = null!;
    private TabModel _selectedTabModel = null!;
    private readonly Vec2 _position = new Vec2(0, 0);

    private BoundingClientRect _rect = null!;
    private bool _dragEventsComing = false;
    private readonly string _mainId = $"_id_{Guid.NewGuid()}";
    private readonly string _extraId = $"_id_{Guid.NewGuid()}";

    private readonly NumberFormatInfo _numberFormatWithDot = new NumberFormatInfo { NumberDecimalSeparator = "." };
    private Action _onDragStart = null!;

    protected override void OnInitialized()
    {
        _onDragStart = async () => { await OnDragStartAsync(); };

        TabContext.OnTabOpen += OnTabSelected;
        TabContext.OnDragContent += OnDragContent;
        TabContextModel.OnDragEnd += OnDragEnd;
        TabContextModel.OnDragStart += _onDragStart;

        _jsInProcessRuntime = (IJSInProcessRuntime)_jsRuntime;

        _panelModel = TabContext.TabPanels[PanelId];
        _selectedTabModel = _panelModel.SelectedTab;
    }

    public void Dispose()
    {
        TabContext.OnTabOpen -= OnTabSelected;
        TabContext.OnDragContent -= OnDragContent;
        TabContextModel.OnDragEnd -= OnDragEnd;
        TabContextModel.OnDragStart -= _onDragStart;
    }

    private void OnTabSelected(int panelId, TabModel tabModel)
    {
        if (panelId == PanelId)
        {
            _selectedTabModel = tabModel;
            StateHasChanged();
        }
    }

    private void OnDragContent(float x, float y)
    {
        if (_panelModel != TabContext.DraggingTabPanel) return;

        _position.Set(x - TabContext.StartDragPosition.X, y + TabContext.StartDragPosition.Y);

        if (!_dragEventsComing)
        {
            _dragEventsComing = true;

            StateHasChanged();
            return;
        }

        _jsInProcessRuntime.InvokeVoid(
            "bchSetTabContentPosition",
            _panelModel.SelectedTab == TabContext.DraggingTabModel ? _mainId : _extraId,
            GetStyleString()
        );
    }

    private async Task OnDragStartAsync()
    {
        //Console.WriteLine($"MTabContent-{PanelId} OnDragStartAsync width" + _rect.Width);

        _rect = await _jSUtilsService.GetBoundingClientRectAsync(_mainId);
    }

    private void OnDragEnd()
    {
        _rect = null!;
        _dragEventsComing = false;
        _position.Set(0, 0);
        StateHasChanged();

        //Console.WriteLine($"MTabContent-{PanelId} OnDragEnd");
    }

    private bool IsMainContentDragging()
        => _rect != null! && _panelModel.SelectedTab == TabContext.DraggingTabModel && _dragEventsComing;

    private bool IsExtraContentDragging()
        => _rect != null! && _panelModel.SelectedTab != TabContext.DraggingTabModel && _dragEventsComing;

    private string GetStyleString()
        => @$"left: {_position.X.ToString(_numberFormatWithDot)}px; 
              top: {_position.Y.ToString(_numberFormatWithDot)}px; 
              width: {_rect.Width.ToString(_numberFormatWithDot)}px; 
              height: {_rect.Height.ToString(_numberFormatWithDot)}px;";
}
