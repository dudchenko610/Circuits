using System.Globalization;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Math;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Circuits.Components.Common.Graph2D;

public partial class BCHGraph2D : IDisposable
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] private IJSUtilsService JsUtilsService { get; set; } = null!;
    
    // [Parameter] public string Width { get; set; } = "600px";
    // [Parameter] public string Height { get; set; } = "450px";

    [Parameter] public int CellHeight { get; set; } = 20;
    [Parameter] public int CellWidth { get; set; } = 20;
    
    [Parameter] public float CellYValues { get; set; } = 0.1f;
    [Parameter] public int CellXValuesCount { get; set; } = 5;
    [Parameter] public IList<float> DataArray { get; set; } = Array.Empty<float>();

    private readonly string _containerId = $"_id_{Guid.NewGuid()}";
    private readonly string _canvasId = $"_id_{Guid.NewGuid()}";
    private readonly NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };

    private float _maxValue = 0;
    
    protected override void OnInitialized()
    {
        IJSUtilsService.OnResize += OnResizeAsync;
        
        // gen data

        const int count = (int) (Math.PI * 4 / 0.05f);
        var arr = new float[count];
        DataArray = arr;

        for (var i = 0; i < count; i++)
        {
            var x = 0.05f * i;
            arr[i] = (float) Math.Cos(x);
        }
        
        // analyze data

        foreach (var value in DataArray)
        {
            var abs = Math.Abs(value);
            if (abs > _maxValue) _maxValue = abs;
        }
    }

    public void Dispose()
    {
        IJSUtilsService.OnResize -= OnResizeAsync;
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        Console.WriteLine("OnAfterRenderAsync");
        
        // if (firstRender)
        // {
        //     var containerRect = await JsUtilsService.GetBoundingClientRectAsync(_containerId);
        //     
        //     await JsRuntime.InvokeVoidAsync("bchInitGraphCanvas", _canvasId, 
        //         new Vec2(containerRect.Width + 100, containerRect.Height),
        //         new Vec2(CellWidth, CellHeight));
        // }
    }

    private async Task OnResizeAsync()
    {
        
    }

    private float SvgHeight => (int) Math.Floor(((2 * _maxValue) / CellYValues) * CellHeight);
    private float SvgWidth => (int) (Math.Floor(DataArray.Count / (double) CellXValuesCount) * CellWidth);
}