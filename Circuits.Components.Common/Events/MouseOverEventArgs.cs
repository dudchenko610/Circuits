using Circuits.ViewModels.Markup;
using Microsoft.AspNetCore.Components.Web;

namespace Circuits.Components.Common.Events;

public class MouseOverEventArgs : MouseEventArgs
{
    public List<CoordsHolder> PathCoordinates { get; set; } = new();
}