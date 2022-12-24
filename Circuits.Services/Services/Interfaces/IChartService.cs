using Circuits.ViewModels.Entities.Charts;
using Circuits.ViewModels.Entities.Equations;

namespace Circuits.Services.Services.Interfaces;

public interface IChartService
{
    Action? OnUpdate { get; set; }
    IReadOnlyList<ChartInfo> Charts { get; }

    void Open(ExpressionVariable variable);
    void Close(ExpressionVariable variable);
}