using System.Globalization;
using Circuits.Components.Common.Events;
using Circuits.Components.Common.Models.Tabs;
using Circuits.Services.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Common.Tabs.Tab;

public partial class BCHTab : IDisposable
{
    [Inject] private IJSUtilsService _iJSUtilsService { get; set; } = null!;

    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    [Parameter] public TabContextModel TabContext { get; set; } = null!;
    [Parameter] public TabModel TabModel { get; set; } = null!;
    [Parameter] public TabPanelModel PanelModel { get; set; } = null!;

    [Parameter] public bool Draggable { get; set; } = true;
    [Parameter] public int Gap { get; set; } = 0;
    [Parameter] public int Height { get; set; } = 0;

    private readonly string _closeIcon = "_content/Circuits.Components.Common/img/tabs/close-tab.svg";
    private readonly string _closeIconSelected = "_content/Circuits.Components.Common/img/tabs/close-tab-selected.svg";

    private bool _dragging = false; // tab that is currently dragging

    private bool _isDragOver = false;
    private bool _direction = false;

    private bool _dragEnded = false;
    private bool _firstDragOver = false;

    private string _updateKey = $"_key_{Guid.NewGuid()}";
    private const bool _shouldRender = true;

    private readonly NumberFormatInfo _numberFormatWithDot = new NumberFormatInfo { NumberDecimalSeparator = "." };

    private async Task OnCloseClickedAsync()
    {
        await OnClose.InvokeAsync();
    }

    protected override void OnParametersSet()
    {
        if (TabModel != null! && TabModel.Pinned)
        {
            Draggable = false;
        }
    }

    protected override bool ShouldRender() => _shouldRender;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        //Console.WriteLine("MTab OnAfterRender " + TabModel.Name);

        if (TabContext.DraggingTabModel == null! && !_dragEnded)
        {
            var rect = await _iJSUtilsService.GetBoundingClientRectAsync(TabModel.Id);
            TabModel.offsetLeft = (float)rect.OffsetLeft;
            TabModel.width = (float)rect.Width;
            TabModel.height = (float)rect.Height;
        }

        _dragEnded = false;
    }

    protected override void OnInitialized()
    {
        TabContextModel.OnDragEnd += OnDragEndExternal;
        TabModel.OnTabDragOver += OnTabDragOver;
        TabModel.OnDraggingTabInvoked += OnDraggingTabInvoked;
    }

    public void Dispose()
    {
        TabContextModel.OnDragEnd -= OnDragEndExternal;
        TabModel.OnTabDragOver -= OnTabDragOver;
        TabModel.OnDraggingTabInvoked -= OnDraggingTabInvoked;
    }

    private void OnDraggingTabInvoked()
    {
        _dragging = true;
        StateHasChanged();
    }

    private void OnTabDragOver(bool isDragOver, bool direction, bool firstDragOver)
    {
        _isDragOver = isDragOver;
        _direction = direction;
        _firstDragOver = firstDragOver;

        StateHasChanged();
    }

    private void OnDragEndExternal()
    {
        _updateKey = $"_key_{Guid.NewGuid()}";

        if (_isDragOver)
        {
            _dragEnded = true;
            _isDragOver = false;
            _firstDragOver = true;
            StateHasChanged();
        }

        if (_dragging) // ???
        {
            TabContext.DraggingTabModel = null!;
            TabContext.DraggingTabPanel = null!;

            _isDragOver = false;
            _dragging = false;
            _dragEnded = true;
            _firstDragOver = true;
            StateHasChanged();
        }
    }

    private void OnDragEnd()
    {
        TabContextModel.OnDragEnd?.Invoke();
        TabContext.DraggingTabModel = null!;
        TabContext.DraggingTabPanel = null!;

        _isDragOver = false;
        _dragging = false;
        _dragEnded = true;
        _firstDragOver = true;
        StateHasChanged();
    }

    private void OnDragStart(MouseStartEventArgs e)
    {
        var tabElementHolder = e.PathCoordinates.FirstOrDefault(x => x.ClassList.Contains("bch-tab-element"));
        if (tabElementHolder == null) return;

        TabContext.StartDragPosition.Set(tabElementHolder.X, TabModel.height - tabElementHolder.Y);

        TabContext.DraggingTabModel = TabModel;
        TabContext.DraggingTabPanel = PanelModel;
        TabContextModel.OnDragStart!?.Invoke();
    }

    private bool IsSelected() => PanelModel.SelectedTab == TabModel;
}
