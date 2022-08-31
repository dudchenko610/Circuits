using Circuits.ViewModels.Events;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Circuits.Components;

[EventHandler("onextscroll", typeof(ScrollEventArgs), true, true)]
[EventHandler("onextmousemove", typeof(ExtMouseEventArgs), true, true)]
[EventHandler("onextmouseup", typeof(ExtMouseEventArgs), true, true)]
[EventHandler("onextmouseout", typeof(ExtMouseEventArgs), true, true)]
[EventHandler("onextmousewheel", typeof(ExtWheelEventArgs), true, true)]
[EventHandler("onmouseleave", typeof(MouseEventArgs), true, true)]
public static class EventHandlers
{
}
