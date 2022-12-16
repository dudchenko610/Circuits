using System.Globalization;
using Circuits.Components.Common.Models.Zoom;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Markup;
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
    
    [Parameter] public float CellYValues { get; set; } = 3f;
    [Parameter] public int CellXValuesCount { get; set; } = 2;
    [Parameter] public IList<double> DataArray { get; set; } = Array.Empty<double>();
    [Parameter] public Func<int, string> XLabel { get; set; } = (val) => $"{val}";
    [Parameter] public Func<int, string> YLabel { get; set; } = (val) => $"{val}";

    private readonly string _containerId = $"_id_{Guid.NewGuid()}";
    private readonly NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    private BoundingClientRect _containerRect = null!;
    private ZoomContext _zoomContext = new();

    private float _maxValue = 0;
    private Vec2 _zoomPos = new();
    private Vec2 _zoomBounds = new();
    private float _prevScale = 1.0f;
    
    protected override void OnInitialized()
    {
        IJSUtilsService.OnResize += OnResizeAsync;
        _zoomContext.OnUpdate += OnZoomUpdate;
        // gen data

        // const int count = (int) (Math.PI * 18 / 0.1f);
        // var arr = new double[count];
        // DataArray = arr;
        //
        // for (var i = 0; i < count; i++)
        // {
        //     var x = 0.1f * i;
        //     // arr[i] = (float)Math.Cos(x * 2) * 0.2f * x;
        //     arr[i] = (float)Math.Cos(x) * 0.5f;
        // }
        
        // analyze data
        foreach (var value in DataArray)
        {
            var abs = Math.Abs(value);
            if (abs > _maxValue) _maxValue = (float) abs;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _containerRect = await JsUtilsService.GetBoundingClientRectAsync(_containerId);
            _zoomBounds.Set(_containerRect.Width, _containerRect.Height);
            StateHasChanged();
        }
        
        // Console.WriteLine("OnAfterRenderAsync");
    }
    
    public void Dispose()
    {
        IJSUtilsService.OnResize -= OnResizeAsync;
        _zoomContext.OnUpdate -= OnZoomUpdate;
    }

    private async Task OnResizeAsync()
    {
        _containerRect = await JsUtilsService.GetBoundingClientRectAsync(_containerId);
        StateHasChanged();
    }

    private void OnZoomUpdate()
    {
        if (_containerRect == null!) return;

        var prevX = _zoomPos.X;
        var prevY = _zoomPos.Y;
        
        var x = Math.Abs(_zoomContext.TopLeftPos.X / _zoomContext.Scale);
        var y = Math.Abs(_zoomContext.TopLeftPos.Y / _zoomContext.Scale);

        var width = (float) _containerRect.Width / _zoomContext.Scale;
        var height = (float) _containerRect.Height / _zoomContext.Scale;

        _zoomPos.Set(x, y);
        _zoomBounds.Set(width, height);

        // TODO: check bound in advance
        if (_prevScale != _zoomContext.Scale || prevX != _zoomPos.X || prevY != _zoomPos.Y) StateHasChanged();

        _prevScale = _zoomContext.Scale;
    }
    
    private float SvgHeight
    {
        get
        {
            float value = (int) Math.Floor((double)((int) ((2 * _maxValue) / CellYValues) * CellHeight)) + 2 * CellHeight;

            if (_containerRect != null! && value < _containerRect.Height)
            {
                value = (CellHeight) * (int) (_containerRect.Height / CellHeight) + CellHeight;
            }

            if ((value / CellHeight) % 2 != 0) value += CellHeight;
            
            return value;
        }
    }

    private float SvgWidth
    {
        get
        {
            float value = (int) (Math.Floor(DataArray.Count / (float) CellXValuesCount) * CellWidth);
            if (_containerRect != null! && value < _containerRect.Width) value = (float) _containerRect.Width;

            return value;
        }
    }
}