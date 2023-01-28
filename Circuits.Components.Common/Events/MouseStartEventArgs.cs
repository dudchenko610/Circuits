using Circuits.ViewModels.Markup;
using Microsoft.AspNetCore.Components.Web;

namespace Circuits.Components.Common.Events;

public class MouseStartEventArgs : MouseEventArgs
{
    public List<CoordsHolder> PathCoordinates { get; set; } = new();
}
