using Circuits.ViewModels.Events;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Circuits.Components;

[EventHandler("onextmousemove", typeof(ExtMouseEventArgs), true, true)]
[EventHandler("onextmouseup", typeof(ExtMouseEventArgs), true, true)]
[EventHandler("onextmouseout", typeof(ExtMouseEventArgs), true, true)]
[EventHandler("onmouseleave", typeof(MouseEventArgs), true, true)]

[EventHandler("onextmousewheel", typeof(ExtWheelEventArgs), true, true)]
[EventHandler("onextscroll", typeof(ScrollEventArgs), true, true)]

[EventHandler("onextdragstart", typeof(ExtMouseEventArgs), true, true)]
[EventHandler("onextdragend", typeof(MouseEventArgs), true, true)]
[EventHandler("onextdragover", typeof(ExtMouseEventArgs), true, true)]
public static class EventHandlers
{
}
