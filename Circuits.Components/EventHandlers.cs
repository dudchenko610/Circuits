using Circuits.ViewModels.Events;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components;

[EventHandler("onextscroll", typeof(ScrollEventArgs), true, true)]
[EventHandler("onextmousemove", typeof(ExtMouseEventArgs), true, true)]
[EventHandler("onextmousewheel", typeof(ExtWheelEventArgs), true, true)]
public static class EventHandlers
{
}
