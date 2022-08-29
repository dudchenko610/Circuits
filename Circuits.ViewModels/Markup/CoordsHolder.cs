using System;
namespace Circuits.ViewModels.Markup;

public class CoordsHolder
{
    public string Id { get; set; } = string.Empty;
    public string ClassList { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float ScrollTop { get; set; }
}