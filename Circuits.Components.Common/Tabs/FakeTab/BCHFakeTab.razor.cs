using System.Globalization;
using Circuits.Components.Common.Models.Tabs;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Common.Tabs.FakeTab;

public partial class BCHFakeTab : IDisposable
{
    private readonly string _closeIcon = "_content/Circuits.Components.Common/img/tabs/close-tab.svg";
    private readonly string _closeIconSelected = "_content/Circuits.Components.Common/img/tabs/close-tab-selected.svg";

    [Parameter] public TabContextModel TabContext { get; set; } = null!;
    [Parameter] public TabModel TabModel { get; set; } = null!;
    [Parameter] public TabPanelModel PanelModel { get; set; } = null!;
    [Parameter] public string CloseImage { get; set; } = string.Empty;

    private readonly NumberFormatInfo _numberFormatWithDot = new NumberFormatInfo { NumberDecimalSeparator = "." };
    private bool IsSelected() => PanelModel.SelectedTab == TabModel;

    private bool _shouldRender = true;
    protected override bool ShouldRender() => _shouldRender;

    protected override void OnAfterRender(bool firstRender)
    {
        //Console.WriteLine("MFakeTab OnAfterRender");
        _shouldRender = false;
    }

    protected override void OnInitialized()
    {
        TabContextModel.OnDragStart += OnDragStart;
    }

    public void Dispose()
    {
        TabContextModel.OnDragStart -= OnDragStart;
    }

    private void OnDragStart()
    {
        _shouldRender = true;
    }
}