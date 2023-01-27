using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Charts;
using Circuits.ViewModels.Entities.Equations;

namespace Circuits.Services.Services;

public class ChartService : IChartService
{
    public Action? OnUpdate { get; set; }
    public IReadOnlyList<ChartInfo> Charts { get; }

    private readonly List<ChartInfo> _charts = new();
    private readonly IWorkerService _workerService;
    private readonly ISchemeService _schemeService;

    public ChartService(IWorkerService workerService, ISchemeService schemeService)
    {
        _workerService = workerService;
        _schemeService = schemeService;

        Charts = _charts;
    }

    public void Open(object key, ExpressionVariable variable, string verticalLetter, Func<float, float>? funcModifier = null!)
    {
        var info = Charts.FirstOrDefault(x => x.Key == key && x.VerticalLetter == verticalLetter);
        if (info != null) return;

        var equationSystem = _schemeService.EquationSystems
            .FirstOrDefault(
                x => x.Variables.Contains(variable) || 
                x.Variables.OfType<ExpressionDerivative>().Any(d => d.Variable == variable)); 
        // it is not recursive, but should be okay with 2nd order max depth
        
        if (equationSystem == null) return;

        if (!_workerService.SolverState.TryGetValue(equationSystem, out var solverState)) return;

        var chartInfo = new ChartInfo
        {
            Key = key,
            Variable = variable,
            EquationSystem = equationSystem,
            SolverState = solverState,
            VerticalLetter = verticalLetter,
            FuncModifier = funcModifier ?? (x => x)
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