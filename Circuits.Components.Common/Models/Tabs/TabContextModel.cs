using Circuits.Shared.Extensions;
using Circuits.ViewModels.Math;

namespace Circuits.Components.Common.Models.Tabs;

public class TabContextModel
{
    public bool Orderable { get; set; } = true;

    public event Action<int, TabModel> OnTabOpen = null!;
    public event Action<int, TabModel> OnTabClose = null!;

    public TabPanelModel[] TabPanels { get; set; } = new TabPanelModel[3];

    internal string ClassIdentifier { get; set; } = $"_context_{Guid.NewGuid()}";

    internal bool Direction { get; set; } = true;
    internal TabModel PrevDragOverModel { get; set; } = null!;
    internal TabModel DraggingTabModel { get; set; } = null!;
    internal TabPanelModel DraggingTabPanel { get; set; } = null!;
    internal Vec2 StartDragPosition { get; set; } = new();

    internal Action? OnUpdateTabPanels { get; set; }
    static internal Action? OnDragStart { get; set; }
    static internal Action? OnDragEnd { get; set; }
    internal Action<TabModel, bool>? SelectTabInPanelAction { get; set; }
    internal Action<float, float>? OnDragContent { get; set; }

    private int _panelCount = 1;

    public TabContextModel() : this(1) { }

    public TabContextModel(int panelCount)
    {
        _panelCount = panelCount;
        TabPanels = new TabPanelModel[_panelCount];

        for (int i = 0; i < _panelCount; i++)
        {
            TabPanels[i] = new TabPanelModel();
        }
    }

    internal void InvokeExternalOnTabOpen(TabModel model, int panel = 0, bool newlySelected = false)
    {
        if (OnTabOpen != null)
        {
            OnTabOpen.Invoke(panel, model);
        }
    }

    public void OpenTab(TabModel model, int panel = 0, bool insertAtBegining = false, bool invokeParent = false)
    {
        var tabPanel = TabPanels[panel];
        var newlySelected = tabPanel.SelectedTabs.LastOrDefault() != model;

        model.TabTemplate = tabPanel.TabTemplate;

        if (!tabPanel.TabModels.Contains(model))
        {
            newlySelected = true;

            if (insertAtBegining)
            {
                tabPanel.TabModels.Insert(0, model);
            }
            else
            {
                tabPanel.TabModels.Add(model);
            }
        }

        if (tabPanel.SelectedTabs.LastOrDefault() != model)
        {
            tabPanel.SelectedTabs.Add(model);
        }

        SelectTabInPanelAction?.Invoke(model, invokeParent);

        if (OnTabOpen != null)
        {
            OnTabOpen.Invoke(panel, model);
        }
    }

    public void CloseTab(TabModel tabModel, TabPanelModel tabPanelModel)
    {
        int panelId = Array.IndexOf(TabPanels, tabPanelModel);

        tabPanelModel.TabModels.Remove(tabModel);

        var preSelected = tabPanelModel.SelectedTabs.Count > 1 ? tabPanelModel.SelectedTabs[tabPanelModel.SelectedTabs.Count - 2] : null; // take before last one or default
        tabPanelModel.SelectedTabs.RemoveAll(x => x == tabModel);
        tabPanelModel.SelectedTabs.EliminateGroupedCopies();

        if (tabPanelModel.SelectedTab == tabModel)
        {
            var tabModelToBeSelected = preSelected ?? tabPanelModel.TabModels.FirstOrDefault()!;

            SelectTabInPanelAction?.Invoke(tabModelToBeSelected, true);

            if (OnTabOpen != null)
            {
                OnTabOpen.Invoke(panelId, tabModelToBeSelected);
            }
        }

        if (OnTabClose != null)
        {
            OnTabClose.Invoke(panelId, tabModel);
        }
    }
}