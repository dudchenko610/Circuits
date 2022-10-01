using Circuits.ViewModels.Markup;
using Microsoft.JSInterop;

namespace Circuits.Services.Services.Interfaces;

public interface IJSUtilsService
{
    Task<BoundingClientRect> GetBoundingClientRectAsync(string id);
    Task ScrollToAsync(string id, string x, string y, string behavior = "smooth"); // auto

    static event Func<Task> OnResize = null!;

    [JSInvokable]
    static async Task OnBrowserResizeAsync()
    {
        if (OnResize != null!)
        {
            await OnResize.Invoke();
        }
    }
}

