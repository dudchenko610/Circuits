using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Common.Models.Tabs;

public class TabPanelModel
{
    public List<TabModel> TabModels { get; set; } = new();
    public TabModel SelectedTab { get; set; } = null!;
    public List<TabModel> SelectedTabs { get; set; } = new();
    internal RenderFragment<TabModel> TabTemplate { get; set; } = null!;
    internal int Height { get; set; }
}