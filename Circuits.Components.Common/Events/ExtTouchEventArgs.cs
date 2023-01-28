using Circuits.ViewModels.Markup;
using Circuits.ViewModels.Rendering;

namespace Circuits.Components.Common.Events;

public class ExtTouchEventArgs : EventArgs
{
    public List<ExtTouchPoint> Touches { get; set; } = new();
    public List<CoordsHolder> PathCoordinates { get; set; } = new();
}