using Circuits.Components.Common.Events;
using Circuits.Components.Common.Models.Tabs;
using Circuits.ViewModels.Math;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Circuits.Components.Common.Tabs.TabContext;

public partial class BCHTabContext : IDisposable
{
    [Inject] private IJSRuntime _jsRuntime { get; set; } = null!;

    [Parameter] public TabContextModel TabContext { get; set; } = null!;
    [Parameter] public RenderFragment ChildContent { get; set; } = null!;

    private IJSInProcessRuntime _jsInProcessRuntime = null!;

    private bool _shouldRender = true;
    private bool _firstDragOver = true;
    private string _prevHoveredElementId = "";
    private float _prevLocalX = 0;

    private readonly Vec2 _prevPos = new Vec2(-1, -1);
    private readonly Vec2 _prevPosContent = new Vec2(-1, -1);

    private readonly string _id = $"_id{Guid.NewGuid()}";
    private DotNetObjectReference<BCHTabContext> _dotNetObjectReference = null!;

    protected override bool ShouldRender() => _shouldRender;

    protected override void OnInitialized()
    {
        TabContextModel.OnDragStart += OnDragStart;
        TabContextModel.OnDragEnd += OnDragEnd;

        _jsInProcessRuntime = (IJSInProcessRuntime)_jsRuntime;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetObjectReference = DotNetObjectReference.Create(this);
            await _jsRuntime.InvokeVoidAsync("bchSubscribeOnTabsDragOver", _id, _dotNetObjectReference);
        }
    }

    public void Dispose()
    {
        TabContextModel.OnDragStart -= OnDragStart;
        TabContextModel.OnDragEnd -= OnDragEnd;
    }

    [JSInvokable]
    public void OnDragOverContainer(MouseOverEventArgs e)
    {
        if (TabContext.DraggingTabModel == null!) return;

        if (_firstDragOver)
        {
            TabContext.DraggingTabModel.OnDraggingTabInvoked?.Invoke();
        }

        HandleTabPanelHovering(e);

        if ((int) _prevPosContent.X == (int) e.PageX && (int) _prevPosContent.Y == (int) e.PageY) return;
        _prevPosContent.Set(e.PageX, e.PageY);

        var contextHolder = e.PathCoordinates.FirstOrDefault(
            x => x.ClassList.Contains("bch-tab-dragzone-container") &&
            x.ClassList.Contains(TabContext.ClassIdentifier)
        );

        if (contextHolder == null) return;

        TabContext.OnDragContent?.Invoke(contextHolder.X, contextHolder.Y);
    }

    private void HandleTabPanelHovering(MouseOverEventArgs e)
    {
        var tabPanelHolder = e.PathCoordinates.FirstOrDefault(
            x => x.ClassList.Contains("bch-tabs-panel") &&
            x.ClassList.Contains(TabContext.ClassIdentifier)
        );

        if (tabPanelHolder == null)
        {
            if (TabContext.PrevDragOverModel != null!) // DRAG LEAVE
            {
                TabContext.PrevDragOverModel?.SendDragOverEvent(false, false, false);
                TabContext.PrevDragOverModel = null!;
                TabContext.Direction = true;
                _prevHoveredElementId = "";
            }

            return;
        }

        var tabElementHolder = e.PathCoordinates.FirstOrDefault(
            x => x.ClassList.Contains("bch-tab-element") &&
            x.ClassList.Contains(TabContext.ClassIdentifier)
        );

        if (tabElementHolder == null) return;

        int panelId = int.Parse(tabPanelHolder.ClassList.Split(" ")[2].Replace("bch-panel-id-", ""));
        var tabModels = TabContext.TabPanels[panelId].TabModels;

        var dragOverModel = tabModels.FirstOrDefault(x => x.Id == tabElementHolder.Id);
        if (dragOverModel == null) return;

        var dragOverDragging = dragOverModel.Id == TabContext.DraggingTabModel.Id;

        if ((int)_prevPos.X == (int) e.PageX && (int) _prevPos.Y == (int) e.PageY) return;

        _prevPos.Set(e.PageX, e.PageY);

        float localX = tabElementHolder.X;

        if (_prevHoveredElementId != tabElementHolder.Id || string.IsNullOrEmpty(_prevHoveredElementId))
        {
            _prevLocalX = localX;
        }

        float width = dragOverModel.width;
        float draggingWidth = TabContext.DraggingTabModel.Width;

        var newDirection = TabContext.Direction;
        var allowChange = Math.Abs(localX - _prevLocalX) > 0.75f;
        var moveToRight = (localX - _prevLocalX) > 0; // to right

        if (allowChange && TabContext.Direction && localX > draggingWidth && moveToRight)
        {
            newDirection = false; // right
        }

        if (allowChange && !TabContext.Direction && localX < dragOverModel.width && !moveToRight)
        {
            newDirection = true; // left
        }

        var directionChanged = newDirection != TabContext.Direction;
        var indexChanged = TabContext.PrevDragOverModel != dragOverModel;

        if (directionChanged)
        {
            TabContext.Direction = newDirection;
        }

        if (indexChanged)
        {
            TabContext.Direction = (dragOverDragging || TabContext.PrevDragOverModel == null!) || TabContext.Direction;

            if (dragOverDragging)
            {
                int index = tabModels.IndexOf(dragOverModel) + 1;
                dragOverModel = index < tabModels.Count ? tabModels[index] : null;
            }

            TabContext.PrevDragOverModel?.SendDragOverEvent(false, false, _firstDragOver);
            TabContext.PrevDragOverModel = dragOverModel!;
        }

        if (directionChanged || indexChanged)
        {
            dragOverModel?.SendDragOverEvent(true, TabContext.Direction, _firstDragOver);
            _firstDragOver = false;
            StateHasChanged(); // ???
        }

        _prevLocalX = localX;
        _prevHoveredElementId = tabElementHolder.Id;
    }

    private void OnDragStart()
    {
        if (TabContext.DraggingTabModel != null!)
        {
            _jsInProcessRuntime.InvokeVoid("bchAddTabDraggingClass", _id);
        }

        _shouldRender = false;
    }

    private void OnDragEnd()
    {
        _jsInProcessRuntime.InvokeVoidAsync("bchRemoveTabDraggingClass", _id);

        _shouldRender = true;
        _prevPos.Set(-1, -1);
        _prevPosContent.Set(-1, -1);
        _firstDragOver = true;
        _prevHoveredElementId = "";

        TabContext.PrevDragOverModel = null!;
        TabContext.Direction = true;
    }
}