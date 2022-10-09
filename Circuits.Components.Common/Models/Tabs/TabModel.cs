using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Common.Models.Tabs;

public class TabModel
{
    public string Id { get; } = $"_id{Guid.NewGuid()}";
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public float Width { get; set; } = 118;
    public float Height { get; set; } = 35;
    public object Payload { get; set; } = null!;
    public bool Pinned { get; set; } = false;


    public string IconImage { get; set; } = "_content/Circuits.Components.Common/img/tabs/default-icon/default-tab.svg";
    public string SelectedIconImage { get; set; } = "_content/Circuits.Components.Common/img/tabs/default-icon/default-tab-selected.svg";
    public bool Closable { get; set; } = true;

    internal float offsetLeft { get; set; } = 0;
    internal float width { get; set; } = 0;
    internal float height { get; set; } = 0;

    internal Action? OnDraggingTabInvoked { get; set; }
    internal Action<bool, bool, bool>? OnTabDragOver { get; set; }
    internal RenderFragment<TabModel> TabTemplate { get; set; } = null!;

    internal void SendDragOverEvent(bool isDragOver, bool direction, bool firstDragOver)
    {
        if (OnTabDragOver != null!)
        {
            OnTabDragOver.Invoke(isDragOver, direction, firstDragOver);
        }
    }
}