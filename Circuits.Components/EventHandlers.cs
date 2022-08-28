using Circuits.ViewModels.Events;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components;

[EventHandler("onextscroll", typeof(ScrollEventArgs), true, true)]
public static class EventHandlers
{
}
