using System.Globalization;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Equations;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeInspector.EquationsInspector;

public partial class EquationsInspectorComponent
{
    [Inject] private IEquationSystemService _equationSystemService { get; set; } = null!;

    private static ExpressionVariable _e = new() { Label = "ε" };
    private static ExpressionVariable _i1 = new() { Label = "i₁" };
    private static ExpressionVariable _i2 = new() { Label = "i₂" };
    private static ExpressionVariable _i3 = new() { Label = "i₃" };
    private static ExpressionVariable _uc = new() { Label = "Uc₁" };
    
    private ExpressionDerivative _i1Derivative = new() { Variable = _i1, Label = "d(i₁)/dt" };
    private ExpressionDerivative _ucDerivative = new() { Variable = _uc, Label = "d(Uc₁)/dt" };

    private double _l = 1.0;
    private double _c = 1.0;
    private double _r = 1.0;

    private EquationSystem _equationSystem;
    private string _eliminationStatus = "Success";
    
    private readonly NumberFormatInfo _nF = new NumberFormatInfo { NumberDecimalSeparator = "." };

    protected override void OnInitialized()
    {
        OnClear();
    }

    private void OnPerformKirchhoffElimination()
    {
        _eliminationStatus = _equationSystemService.PerformKirchhoffElimination(_equationSystem);
        StateHasChanged();
    }

    private void OnClear()
    {
        _eliminationStatus = "Success";
        _equationSystem = new EquationSystem(_i2, _i3, _i1Derivative, _ucDerivative)
        {
            Matrix = new []
            {
                new Expression[] { new ExpressionValue(-1), new ExpressionValue(-1), new ExpressionValue(0), new ExpressionValue(0), -_i1 },
                new Expression[] { new ExpressionValue(1), new ExpressionValue(0), new ExpressionValue(_l), new ExpressionValue(0), _e - (_i1 * _r) },
                new Expression[] { new ExpressionValue(0), new ExpressionValue(0), new ExpressionValue(0), new ExpressionValue(_c), -_uc },
                new Expression[] { new ExpressionValue(0), new ExpressionValue(0), new ExpressionValue(_l), new ExpressionValue(0), _i1 },
            }
        };
        
        StateHasChanged();
    }
}