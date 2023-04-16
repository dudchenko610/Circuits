using BlazorComponentHeap.Core.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Circuits.Components.Main.SchemeInspector;

public partial class SchemeInspectorComponent
{
    class TabModel
    {
        public string Name { get; set; }
        public int Width { get; set; }
    }

    [Inject] private IJSUtilsService JsUtilsService { get; set; } = null!;

    [Parameter] public EventCallback<bool> OpenedChanged { get; set; }

    [Parameter]
    public bool Opened
    {
        get => _opened;
        set
        {
            if (_opened == value) return;
            _opened = value;

            OpenedChanged.InvokeAsync(value);
        }
    }

    private bool _isMouseDown = false;
    private bool _opened = false;

    private readonly List<TabModel> _tabModels = new List<TabModel>()
    {
        new()
        {
            Name = $"Graph Inspector",
            Width = 215,
        },
        new()
        {
            Name = $"Equations Inspector",
            Width = 215,
        },
        new()
        {
            Name = $"Solver Inspector",
            Width = 215,
        },
        new()
        {
            Name = $"Storage",
            Width = 150,
        }
    };
    
    private readonly string _key = $"_key_{Guid.NewGuid()}";
    private int _blockWidth = 450;

    // protected override void OnAfterRender(bool firstRender)
    // {
    //     Console.WriteLine("ElementDetails OnAfterRender");
    // }

    private async Task OnMouseDownAsync()
    {
        _isMouseDown = true;

        await JsUtilsService.AddDocumentListenerAsync<MouseEventArgs>("mouseup", _key, OnMouseUpAsync);
        await JsUtilsService.AddDocumentListenerAsync<MouseEventArgs>("mousemove", _key, OnMouseMoveAsync);
    }

    [JSInvokable]
    public Task OnMouseMoveAsync(MouseEventArgs args)
    {
        _blockWidth = Math.Clamp((int)args.ClientX - 16, 300, 1000);
        StateHasChanged();

        return Task.CompletedTask;
    }

    [JSInvokable]
    public async Task OnMouseUpAsync(MouseEventArgs _)
    {
        _isMouseDown = false;

        await JsUtilsService.RemoveDocumentListenerAsync<MouseEventArgs>("mouseup", _key);
        await JsUtilsService.RemoveDocumentListenerAsync<MouseEventArgs>("mousemove", _key);
    }
}