using System.Globalization;
using Circuits.ViewModels.Entities.Equations;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeInspector.EquationsInspector.GraphEquationSystem;

public partial class GraphEquationSystemComponent
{
    [Parameter] public EquationSystem EquationSystem { get; set; } = null!;
    
    private readonly NumberFormatInfo _nF = new()
    {
        NumberDecimalSeparator = ".",
        NumberDecimalDigits = 5
    };
}