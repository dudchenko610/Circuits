using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Charts;
using Circuits.ViewModels.Entities.Equations;

namespace Circuits.Services.Services;

public class ChartService : IChartService
{
    public Action? OnUpdate { get; set; }
    public IReadOnlyList<ChartInfo> Charts { get; }

    private readonly List<ChartInfo> _charts = new();
    private readonly ISolverService _solverService;
    private readonly ISchemeService _schemeService;

    public ChartService(ISolverService solverService, ISchemeService schemeService)
    {
        _solverService = solverService;
        _schemeService = schemeService;

        Charts = _charts;
    }

    public void Open(ExpressionVariable variable, string verticalLetter)
    {
        var info = Charts.FirstOrDefault(x => x.Variable == variable);
        if (info != null) return;

        var equationSystem = _schemeService.EquationSystems
            .FirstOrDefault(
                x => x.Variables.Contains(variable) || 
                x.Variables.OfType<ExpressionDerivative>().Any(d => d.Variable == variable)); 
        // it is not recursive, but should be okay with 2nd order max depth
        
        if (equationSystem == null) return;

        if (!_solverService.SolverState.TryGetValue(equationSystem, out var solverState)) return;

        var chartInfo = new ChartInfo
        {
            Variable = variable,
            EquationSystem = equationSystem,
            SolverState = solverState,
            VerticalLetter = verticalLetter
        };

        _charts.Add(chartInfo);
        OnUpdate?.Invoke();
    }

    public void Close(ExpressionVariable variable)
    {
        var chartInfo = Charts.FirstOrDefault(x => x.Variable == variable);
        
        if (chartInfo != null && !_charts.Remove(chartInfo)) return;
        OnUpdate?.Invoke();
    }
}