using System.Globalization;
using Circuits.Components.Common.Models.Tabs;
using Circuits.Services.Services.Interfaces;
using Circuits.Shared.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Circuits.Components.Common.Tabs.TabPanel;

public partial class BCHTabPanel : IDisposable
{
    [Inject] private IJSUtilsService _jsUtilsService { get; set; } = null!;

    [Parameter] public TabContextModel TabContext { get; set; } = null!;
    [Parameter] public bool SelectOnDragAndDrop { get; set; } = false;
    [Parameter] public int PanelId { get; set; } = 0;
    [Parameter] public int MinimalTabsCount { get; set; } = 1;
    [Parameter] public float ScrollTabWidth { get; set; } = 100;
    [Parameter] public int Height { get; set; } = 35;
    [Parameter] public bool ScrollToCenterOnClick { get; set; } = false;
    [Parameter] public EventCallback<TabModel> OnClose { get; set; }
    [Parameter] public EventCallback<(TabModel, MouseEventArgs)> OnIconClick { get; set; }

    private bool _leftReached;
    private bool _rightReached;

    [Parameter] public int Gap { get; set; } = 4;

    [Parameter] public EventCallback<bool> LeftReachedChanged { get; set; }
    [Parameter] public EventCallback<bool> RightReachedChanged { get; set; }

    [Parameter]
    public bool LeftReached
    {
        get => _leftReached;
        set
        {
            if (_leftReached == value) return;
            _leftReached = value;
            LeftReachedChanged.InvokeAsync(value);
        }
    }
    [Parameter]
    public bool RightReached
    {
        get => _rightReached;
        set
        {
            if (_rightReached == value) return;
            _rightReached = value;
            RightReachedChanged.InvokeAsync(value);
        }
    }

    private TabModel _draggingTabModel = null!;
    [Parameter] public EventCallback<TabModel> DraggingTabModelChanged { get; set; }
    [Parameter]
    public TabModel DraggingTabModel
    {
        get => _draggingTabModel;
        set
        {
            if (_draggingTabModel == value) return;
            _draggingTabModel = value;
            DraggingTabModelChanged.InvokeAsync(value);
        }
    }

    private TabModel _selectedTab
    {
        get => TabContext.TabPanels[PanelId].SelectedTab;
        set => TabContext.TabPanels[PanelId].SelectedTab = value!;
    }

    [Parameter] public EventCallback<TabModel> SelectedTabChanged { get; set; }
    [Parameter]
    public TabModel SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (_selectedTab == value) return;
            _selectedTab = value;
            SelectedTabChanged.InvokeAsync(value);
        }
    }

    [Parameter] public RenderFragment<TabModel> TabTemplate { get; set; } = null!;

    private readonly NumberFormatInfo _numberFormatWithDot = new NumberFormatInfo { NumberDecimalSeparator = "." };

    private TabPanelModel _panel = new();
    private readonly string _tabContainerId = $"_id{Guid.NewGuid()}";
    private readonly string _tabDraggableId = $"_id{Guid.NewGuid()}";

    private float _scrollOffset = 0;
    private string _scrollStr = "0";
    private bool _showLeft = false;
    private bool _showRight = false;

    private bool _shouldRender = true;

    private string _updateKey = $"_key_{Guid.NewGuid()}";

    private Action _onUpdateTabPanel = null!;

    protected override void OnInitialized()
    {
        _onUpdateTabPanel = async () => { await OnUpdateTabPanelAsync(); };

        IJSUtilsService.OnResize += UpdateScrollAsync;
        IJSUtilsService.OnResize += UpdateControlButtonsAsync;
        TabContextModel.OnDragStart += OnDragStart;
        TabContextModel.OnDragEnd += OnDragEnd;
        TabContext.OnUpdateTabPanels += _onUpdateTabPanel;

        MinimalTabsCount = MinimalTabsCount < 1 ? 1 : MinimalTabsCount;

        _panel = TabContext.TabPanels[PanelId];
        _panel.TabTemplate = TabTemplate;
        _panel.Height = Height;

        foreach (var tm in _panel.TabModels)
        {
            tm.TabTemplate = TabTemplate;
        }

        if (SelectedTab == null!)
        {
            SelectedTab = _panel.TabModels[0];
        }

        OnSelect(SelectedTab);

        TabContext.SelectTabInPanelAction += async (tabModel, notifyParent) =>
        {
            if (!_panel.TabModels.Contains(tabModel))
            {
                return;
            }

            if (notifyParent)
            {
                SelectedTab = tabModel;
            }
            else
            {
                _selectedTab = tabModel;
            }

            await UpdateControlButtonsAsync();
            StateHasChanged();
        };
    }

    public void Dispose()
    {
        IJSUtilsService.OnResize -= UpdateScrollAsync;
        IJSUtilsService.OnResize -= UpdateControlButtonsAsync;
        TabContextModel.OnDragStart -= OnDragStart;
        TabContextModel.OnDragEnd -= OnDragEnd;
        TabContext.OnUpdateTabPanels -= _onUpdateTabPanel;
    }

    protected override bool ShouldRender() => _shouldRender;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await UpdateControlButtonsAsync();
        await UpdateScrollAsync();
    }

    private void OnDragStart()
    {
        _shouldRender = false;
    }

    private void OnDragEnd()
    {
        _shouldRender = true;
        StateHasChanged();
    }

    private async Task OnUpdateTabPanelAsync()
    {
        _shouldRender = true;
        _updateKey = $"_key_{Guid.NewGuid()}";

        StateHasChanged();

        await UpdateControlButtonsAsync();
        await UpdateScrollAsync();
    }

    private async Task UpdateControlButtonsAsync()
    {
        var containerRect = await _jsUtilsService.GetBoundingClientRectAsync(_tabContainerId);
        var draggableRect = await _jsUtilsService.GetBoundingClientRectAsync(_tabDraggableId);

        if (containerRect == null! || draggableRect == null!) return;

        var rightValue = Math.Min((float)(containerRect.Width - draggableRect.Width), 0.0f);

        var oldShowLeft = _showLeft;
        var oldShowRight = _showRight;
        
        _showLeft = _scrollOffset != 0.0f;
        _showRight = _scrollOffset != rightValue;

        LeftReached = _showLeft;
        RightReached = _showRight || Math.Abs(_scrollOffset) < containerRect.Width;

        if (oldShowLeft != _showLeft || oldShowRight != _showRight)
        {
            StateHasChanged();
        }
    }

    private async Task OnLeftClickAsync() => await UpdateScrollAsync(ScrollTabWidth);

    private async Task OnRightClickAsync() => await UpdateScrollAsync(-ScrollTabWidth);

    private async Task UpdateScrollAsync() => await UpdateScrollAsync(0);

    private async Task UpdateScrollAsync(float offset)
    {
        var containerRect = await _jsUtilsService.GetBoundingClientRectAsync(_tabContainerId);
        var draggableRect = await _jsUtilsService.GetBoundingClientRectAsync(_tabDraggableId);

        float rightValue = Math.Min((float)(containerRect.Width - draggableRect.Width), 0.0f);

        float newScrollOffset = Math.Clamp(_scrollOffset + offset, rightValue, 0.0f);

        if (newScrollOffset != _scrollOffset)
        {
            _scrollOffset = newScrollOffset;
            _scrollStr = _scrollOffset.ToString(_numberFormatWithDot);

            _showLeft = _scrollOffset != 0.0f;
            _showRight = _scrollOffset != rightValue;

            LeftReached = _showLeft;
            RightReached = _showRight;

            StateHasChanged();
        }
    }

    private async Task ScrollAsync(TabModel tabModel)
    {
        var containerRect = await _jsUtilsService.GetBoundingClientRectAsync(_tabContainerId);
        var draggableRect = await _jsUtilsService.GetBoundingClientRectAsync(_tabDraggableId);

        float rightValue = Math.Min((float)(containerRect.Width - draggableRect.Width), 0.0f);
        float newScrollOffset = Math.Clamp(
            -(tabModel.offsetLeft - (float)(containerRect.Width * 0.5f) + tabModel.width * 0.5f),
            rightValue,
            0.0f
        );

        if (newScrollOffset != _scrollOffset)
        {
            _scrollOffset = newScrollOffset;
            _scrollStr = _scrollOffset.ToString(_numberFormatWithDot);

            _showLeft = _scrollOffset != 0.0f;
            _showRight = _scrollOffset != rightValue;

            LeftReached = _showLeft;
            RightReached = _showRight;

            StateHasChanged();
        }
    }

    private void OnDrop()
    {
        if (TabContext.DraggingTabModel == null!) return;

        var thisTabPanel = TabContext.TabPanels[PanelId];
        var startDragIndex = TabContext.DraggingTabPanel.TabModels.IndexOf(TabContext.DraggingTabModel);
        int actualIndex;

        if (TabContext.DraggingTabPanel == thisTabPanel)
        {
            var dragOverIndex = TabContext.DraggingTabPanel.TabModels.IndexOf(TabContext.PrevDragOverModel);
            actualIndex = dragOverIndex + (TabContext.Direction ? 0 : 1);

            if (actualIndex > startDragIndex && startDragIndex != -1)
            {
                actualIndex--;
            }

            thisTabPanel.TabModels.Remove(TabContext.DraggingTabModel);
        }
        else
        {
            var dragOverIndex = thisTabPanel.TabModels.IndexOf(TabContext.PrevDragOverModel);
            actualIndex = dragOverIndex + (TabContext.Direction ? 0 : 1);

            TabContext.CloseTab(TabContext.DraggingTabModel, TabContext.DraggingTabPanel);
        }

        if (actualIndex == -1) actualIndex = thisTabPanel.TabModels.Count();

        actualIndex = Math.Clamp(actualIndex, 0, thisTabPanel.TabModels.Count());
        thisTabPanel.TabModels.Insert(actualIndex, TabContext.DraggingTabModel); // into this panel

        if (SelectOnDragAndDrop)
        {
            OnSelect(TabContext.DraggingTabModel);
        }

        TabContextModel.OnDragEnd?.Invoke();
        TabContext?.OnUpdateTabPanels?.Invoke();
    }

    private void OnSelect(TabModel tabModel)
    {
        if (_panel.SelectedTabs.LastOrDefault() != tabModel)
        {
            _panel.SelectedTabs.Add(tabModel);
        }

        bool newlySelected = SelectedTab != tabModel;

        SelectedTab = tabModel;
        TabContext.InvokeExternalOnTabOpen(tabModel, PanelId, newlySelected);
        StateHasChanged();
    }

    private async Task OnCloseAsync(TabModel tabModel)
    {
        _panel.TabModels.Remove(tabModel);

        var preSelected = _panel.SelectedTabs.Count > 1 ? _panel.SelectedTabs[^2] : null; // take before last one or default
        _panel.SelectedTabs.RemoveAll(x => x == tabModel);
        _panel.SelectedTabs.EliminateGroupedCopies();

        if (_panel.SelectedTab == tabModel)
        {
            if (preSelected != null)
            {
                OnSelect(preSelected);
                TabContext.InvokeExternalOnTabOpen(preSelected, PanelId, true);
            }
            else
            {
                var first = _panel.TabModels.FirstOrDefault()!;
                OnSelect(first!);
                TabContext.InvokeExternalOnTabOpen(first!, PanelId, true);
            }
        }

        await OnClose.InvokeAsync(tabModel);
        await OnUpdateTabPanelAsync();

        StateHasChanged();
    }
}
