using Circuits.ViewModels.Markup;
using Microsoft.AspNetCore.Components.Web;

namespace Circuits.Components.Common.Events;

public class ExtWheelEventArgs : WheelEventArgs
{
    public float X { get; set; }
    public float Y { get; set; }

    public List<CoordsHolder> PathCoordinates { get; set; } = new();
}