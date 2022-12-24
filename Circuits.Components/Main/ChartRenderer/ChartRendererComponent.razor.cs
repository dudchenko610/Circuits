using Circuits.Services.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.ChartRenderer;

public partial class ChartRendererComponent : IDisposable
{
    [Inject] public IChartService ChartService { get; set; } = null!;

    protected override void OnInitialized()
    {
        ChartService.OnUpdate += StateHasChanged;
    }

    public void Dispose()
    {
        ChartService.OnUpdate -= StateHasChanged;
    }
}