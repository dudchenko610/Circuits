using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Common.Models.Modal;

public class ModalModel
{
    public string Width { get; set; } = string.Empty;
    public string Height { get; set; } = string.Empty;
    public string X { get; set; } = string.Empty;
    public string Y { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public bool Overlay { get; set; } = true;
    public RenderFragment Fragment { get; set; } = null!;
    public Action? OnUpdate { get; set; }
}