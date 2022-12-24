using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Common.ControlButton;

public partial class ControlButtonComponent
{
    [Parameter] public string Label { get; set; } = string.Empty;
    [Parameter] public string Color { get; set; } = "#242426";
    [Parameter] public string BorderColor { get; set; } = "rgba(3, 2, 41, 0.1)";
    [Parameter] public EventCallback OnClick { get; set; }
}