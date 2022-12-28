using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Common.Models.Modal;

public class ModalModel
{
    public string Id { get; } = $"_id_{Guid.NewGuid()}";
    public string CssClass { get; set; } = string.Empty;
    public string Width { get; set; } = string.Empty;
    public string Height { get; set; } = string.Empty;
    public string X { get; set; } = string.Empty;
    public string Y { get; set; } = string.Empty;
    public bool Overlay { get; set; } = true;
    public int ZIndex { get; set; } = 999999;
    public RenderFragment Fragment { get; set; } = null!;
    public Action? OnUpdate { get; set; }
}