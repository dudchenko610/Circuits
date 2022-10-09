using Circuits.Components.Common.Models.Tabs;

namespace Circuits.Components.Main.SchemeInspector;

public partial class SchemeInspectorComponent
{
    private readonly TabContextModel _context = new TabContextModel(1) { Orderable = true };
    
    protected override void OnInitialized()
    {
        _context.TabPanels[0].TabModels.Add(new TabModel
        {
            Type = "graph-inspector",
            Name = $"Graph Inspector",
            Width = 175,
            Height = 35,
            Closable = false,
            IconImage = "_content/Circuits.Components.Common/img/tabs/default-icon/default-tab.svg",
            SelectedIconImage = "_content/Circuits.Components.Common/img/tabs/default-icon/default-tab-selected.svg"
        });
        
        _context.TabPanels[0].TabModels.Add(new TabModel
        {
            Type = "equations-inspector",
            Name = $"Equations Inspector",
            Width = 185,
            Height = 35,
            Closable = false,
            IconImage = "_content/Circuits.Components.Common/img/tabs/default-icon/default-tab.svg",
            SelectedIconImage = "_content/Circuits.Components.Common/img/tabs/default-icon/default-tab-selected.svg"
        });
    }
}