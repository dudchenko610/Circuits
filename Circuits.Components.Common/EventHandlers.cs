using Circuits.Components.Common.Events;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Circuits.Components.Common;

[EventHandler("onextdragenter", typeof(MouseEventArgs), true, true)]
[EventHandler("onextdragstart", typeof(MouseStartEventArgs), true, true)]
[EventHandler("onextdragend", typeof(MouseEventArgs), true, true)]
[EventHandler("onextscroll", typeof(ScrollEventArgs), true, true)]
[EventHandler("onextmousewheel", typeof(ExtWheelEventArgs), true, true)]
[EventHandler("onmouseleave", typeof(ExtMouseEventArgs), true, true)]
public static class EventHandlers
{
}