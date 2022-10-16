using System.Globalization;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Equations;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeInspector.EquationsInspector;

public partial class EquationsInspectorComponent
{
    [Inject] private IEquationSystemService _equationSystemService { get; set; } = null!;
    [Inject] private ISchemeService _schemeService { get; set; } = null!;

    // private string _eliminationStatus = "Success";

    private List<EquationSystem> _equationSystems = new();
    
    protected override void OnInitialized()
    {
        OnClear();
    }

    private void OnBuildEquationSystems()
    {
        _equationSystems = _equationSystemService.BuildEquationSystemsFromGraphs(_schemeService.Graphs);
        StateHasChanged();
    }
    
    private void OnPerformKirchhoffElimination()
    {
        foreach (var equationSystem in _equationSystems)
        {
            _equationSystemService.PerformKirchhoffElimination(equationSystem);
        }
        
        // _eliminationStatus = _equationSystemService.PerformKirchhoffElimination(_equationSystem);
        StateHasChanged();
    }

    private void OnClear()
    {
        // _eliminationStatus = "Success";

        StateHasChanged();
    }
}