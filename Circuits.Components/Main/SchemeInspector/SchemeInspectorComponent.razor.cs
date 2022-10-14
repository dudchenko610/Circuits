using Circuits.Components.Common.Models.Tabs;
using Circuits.Components.Common.Select;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Circuits.Components.Main.SchemeInspector;

public partial class SchemeInspectorComponent : IDisposable
{
    [Inject] private IJSRuntime _jsRuntime { get; set; } = null!;

    [Parameter] public EventCallback<bool> OpenedChanged { get; set; }
    [Parameter] public bool Opened
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
    private readonly TabContextModel _context = new(1) { Orderable = true };
    private DotNetObjectReference<SchemeInspectorComponent> _dotNetRef = null!;
    private readonly string _key = $"_key_{Guid.NewGuid()}";
    private int _blockWidth = 300;
    
    protected override void OnInitialized()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        
        _context.TabPanels[0].TabModels.Add(new TabModel
        {
            Type = "graph-inspector",
            Name = $"Graph Inspector",
            Width = 175,
            Height = 35,
            Closable = false,
            IconImage = "_content/Circuits.Components.Common/img/tabs/default-icon/default-tab.svg",
            SelectedIconImage = "_content/Circuits.Components.Common/img/tabs/default-icon/default-tab-selected.svg"
        });

        _context.TabPanels[0].TabModels.Add(new TabModel
        {
            Type = "equations-inspector",
            Name = $"Equations Inspector",
            Width = 185,
            Height = 35,
            Closable = false,
            IconImage = "_content/Circuits.Components.Common/img/tabs/default-icon/default-tab.svg",
            SelectedIconImage = "_content/Circuits.Components.Common/img/tabs/default-icon/default-tab-selected.svg"
        });
    }
    
    public void Dispose()
    {
        _dotNetRef.Dispose();
    }

    private async Task OnMouseDownAsync()
    {
        _isMouseDown = true;
        
        await _jsRuntime.InvokeVoidAsync("addDocumentListener", _key, "mouseup", _dotNetRef, "OnMouseUpAsync");
        await _jsRuntime.InvokeVoidAsync("addDocumentListener", _key, "mousemove", _dotNetRef, "OnMouseMoveAsync");
    }

    [JSInvokable]
    public Task OnMouseMoveAsync(MouseEventArgs args)
    {
        _blockWidth = Math.Clamp((int) args.ClientX - 16, 300, 1000);
        StateHasChanged();

        return Task.CompletedTask;
    }
    
    [JSInvokable]
    public async Task OnMouseUpAsync()
    {
        _isMouseDown = false;
        
        await _jsRuntime.InvokeVoidAsync("removeDocumentListener", _key, "mouseup");
        await _jsRuntime.InvokeVoidAsync("removeDocumentListener", _key, "mousemove");
    }
}