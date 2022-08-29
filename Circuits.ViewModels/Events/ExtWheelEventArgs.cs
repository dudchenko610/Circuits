using Microsoft.AspNetCore.Components.Web;

namespace Circuits.ViewModels.Events;

public class ExtWheelEventArgs : WheelEventArgs
{
    public float X { get; set; }
    public float Y { get; set; }
}

