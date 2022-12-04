using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Markup;
using Microsoft.JSInterop;

namespace Circuits.Services.Services;

public class JSUtilsService : IJSUtilsService
{
    private readonly IJSRuntime _jsRuntime;

    public JSUtilsService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<BoundingClientRect> GetBoundingClientRectAsync(string id)
    {
        return await _jsRuntime.InvokeAsync<BoundingClientRect>("bchGetBoundingClientRectById", id);
    }

    public async Task ScrollToAsync(string id, string x, string y, string behavior = "smooth")
    {
        await _jsRuntime.InvokeVoidAsync("bchScrollElementTo", id, x, y, behavior);
    }

    public async Task<string> CreateObjectURLAsync(string text)
    {
        return await _jsRuntime.InvokeAsync<string>("createObjectURL", text);
    }

    public async Task RevokeObjectAsync(string url)
    {
        await _jsRuntime.InvokeVoidAsync("URL.revokeObjectURL", url);
    }
}
