using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Math;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Circuits.Components.Common.Graph2D;

public partial class BCHGraph2D
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] private IJSUtilsService JsUtilsService { get; set; } = null!;
    
    [Parameter] public string Width { get; set; } = "600px";
    [Parameter] public string Height { get; set; } = "450px";

    private readonly string _containerId = $"_id_{Guid.NewGuid()}";
    private readonly string _canvasId = $"_id_{Guid.NewGuid()}";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var containerRect = await JsUtilsService.GetBoundingClientRectAsync(_containerId);
            
            await JsRuntime.InvokeVoidAsync("bchInitGraphCanvas", _canvasId, 
                new Vec2(containerRect.Width, containerRect.Height));
        }
    }
}