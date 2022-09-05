using System.Globalization;
using Circuits.Components.Main.SchemeRenderer.Elements;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Events;
using Circuits.ViewModels.Math;
using Circuits.ViewModels.Rendering.Scheme;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Circuits.Components.Main.SchemeRenderer;

public partial class SchemeRendererComponent : IDisposable
{
    [Inject] private IJSRuntime _jsRuntime { get; set; } = null!;
    [Inject] private ISchemeService _schemeService { get; set; } = null!;
    [Parameter] public RenderFragment ChildContent { get; set; } = null!;
    [Parameter] public SchemeRendererContext SchemeRendererContext { get; set; } = null!;
    [Parameter] public float Scale { get; set; } = 1.0f;

    private int CellSize => SchemeRendererContext.CellSize;

    private NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    private Vec2 _elementPointerPos = new(-99999, -99999);

    private Vec2 _firstPointPos = new();
    private bool _firstPointSet = false;
    private bool _firstMouseMove = false;

    public event Action OnDragUpdate = null!;

    public Element SelectedElement { get; private set; } = null!;
    public Element DraggingElement { get; private set;  } = null!;
    public bool FirstDragOver { get; private set; } = false;

    public Vec2 DraggingPos { get; } = new();
    private Vec2 _draggingPosOffset = new();

    private readonly string _id = $"_id_{Guid.NewGuid()}";
    private readonly string _contentId = $"_id_{Guid.NewGuid()}";
    private DotNetObjectReference<SchemeRendererComponent> _dotNetObjectReference = null!;

    private Dictionary<Type, ElementComponent> _elementComponents = new();

    protected override void OnInitialized()
    {
        SchemeRendererContext.OnUpdate += Update;
    }
    
    public void Dispose()
    {
        _dotNetObjectReference?.Dispose();
        SchemeRendererContext.OnUpdate -= Update;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        //Console.WriteLine("OnAfterRender SchemeRendererComponent");

        if (firstRender)
        {
            _dotNetObjectReference = DotNetObjectReference.Create(this);
            await _jsRuntime.InvokeVoidAsync("subscribeOnMouseMove", _id, _dotNetObjectReference);
            await _jsRuntime.InvokeVoidAsync("subscribeOnDragOver", _contentId, _dotNetObjectReference);
        }
    }

    private void Update()
    {
        if (!SchemeRendererContext.PencilMode)
        {
            _elementPointerPos.Set(-99999, -99999);
            _firstPointSet = false;
            _firstMouseMove = false;
        }

        SelectedElement = null!;

        StateHasChanged();
    }
    
    [JSInvokable]
    public void OnMouseMove(ExtMouseEventArgs e)
    {
        if (SchemeRendererContext.PencilMode)
        {
            var schemeRendererContainer =
                e.PathCoordinates.FirstOrDefault(x => x.ClassList.Contains("scheme-renderer-container"));

            if (schemeRendererContainer is null)
                return;

            schemeRendererContainer.X /= Scale;
            schemeRendererContainer.Y /= Scale;

            schemeRendererContainer.X -= 0.5f * CellSize;
            schemeRendererContainer.Y -= 0.5f * CellSize;

            int posX = (int)(schemeRendererContainer.X / CellSize) * CellSize + CellSize;
            int posY = (int)(schemeRendererContainer.Y / CellSize) * CellSize + CellSize;

            if (_elementPointerPos.X != posX || _elementPointerPos.Y != posY)
            {
                if ((int) _elementPointerPos.X != -99999) _firstMouseMove = true;
                
                _elementPointerPos.Set(posX, posY);
                StateHasChanged();
            }
        }
    }

    private void OnContainerDown() => OnFirstPointSet();

    private void OnContainerUp() => OnSecondPointSet();

    private void OnContainerClicked()
    {
        if (SelectedElement == null!) return;

        SelectedElement = null!;
        StateHasChanged();
    }

    private bool OnFirstPointSet()
    {
        if (!SchemeRendererContext.PencilMode || _firstPointSet) return false;

        _firstPointSet = true;
        _firstPointPos.Set(_elementPointerPos);
        StateHasChanged();

        return true;
    }

    private bool OnSecondPointSet()
    {
        if (SchemeRendererContext.PencilMode && _firstPointSet)
        {
            var p1 = new Vec2((int)_firstPointPos.X / CellSize, (int)_firstPointPos.Y / CellSize);
            var p2 = new Vec2((int)_elementPointerPos.X / CellSize, (int)_elementPointerPos.Y / CellSize);

            if ((int) p1.Y == (int) p2.Y && (int) p1.X == (int) p2.X)
                return false;

            // Pizdec below
            if ((int) p1.X == (int) p2.X)
            {
                if (p1.Y < p2.Y)
                {
                    _schemeService.Add(new Wire { P1 = p1, P2 = p2 });
                }
                else
                {
                    _schemeService.Add(new Wire { P1 = p2, P2 = p1 });
                }
            }
            else if (p1.Y == p2.Y)
            {
                if (p1.X < p2.X)
                {
                    _schemeService.Add(new Wire { P1 = p1, P2 = p2 });
                }
                else
                {
                    _schemeService.Add(new Wire { P1 = p2, P2 = p1 });
                }
            }

            SchemeRendererContext.PencilMode = false;

            return true;
        }

        return false;
    }

    [JSInvokable]
    public void OnDragOver(ExtMouseEventArgs e)
    {
        var container = e.PathCoordinates
            .FirstOrDefault(x => x.ClassList.Contains("scheme-renderer-container"));

        if (container is null) return;
        if (!FirstDragOver) FirstDragOver = true;

        DraggingPos.Set((container.X - _draggingPosOffset.X) / Scale, (container.Y - _draggingPosOffset.Y) / Scale);
        OnDragUpdate?.Invoke();
    }

    private void OnDrop(ExtMouseEventArgs e)
    {
        // Console.WriteLine($"on-drop");

        var container = e.PathCoordinates
            .FirstOrDefault(x => x.ClassList.Contains("scheme-renderer-container"));

        if (container is not null)
        {
            var dS = new Vec2(
                (int)((container.X - _draggingPosOffset.X) / Scale + 0.5f * CellSize) / CellSize,
                (int)((container.Y - _draggingPosOffset.Y) / Scale + 0.5f * CellSize) / CellSize
            ).Add(-DraggingElement.Points[0].X, -DraggingElement.Points[0].Y);
            
            DraggingElement.Translate(dS);
            
            if (!_schemeService.Intersects(DraggingElement))
            {
                _schemeService.Remove(DraggingElement);
                _schemeService.Add(DraggingElement);
            }
            else
            {
                DraggingElement.Translate(dS.Multiply(-1));
            }
        }

        FirstDragOver = false;
        DraggingElement = null!;
        StateHasChanged();
    }

    public void OnDragStart(ExtMouseEventArgs e, Wire wire)
    {
        var wireCnt = e.PathCoordinates
            .FirstOrDefault(x => x.ClassList.Contains("wire"));

        if (wireCnt == null) return;

        _draggingPosOffset.Set(wireCnt.X, wireCnt.Y);
        DraggingElement = wire;

        // Console.WriteLine($"on-drag-start X: {wireCnt.X}, Y: {wireCnt.Y}");
    }

    public void OnDragEnd(ExtMouseEventArgs e)
    {
        // Console.WriteLine($"on-drag-end");

        if (FirstDragOver)
        {
            var container = e.PathCoordinates
                .FirstOrDefault(x => x.ClassList.Contains("scheme-renderer-container"));

            if (container != null)
            {
                var lastPos = new Vec2();
                lastPos.Set(DraggingPos).Multiply(Scale).Add(_draggingPosOffset);

                container.X = lastPos.X;
                container.Y = lastPos.Y;
                
                OnDrop(e);
                return;
            }
        }

        FirstDragOver = false;
        DraggingElement = null!;
        StateHasChanged();
    }

    public void OnElementClicked(Element element)
    {
        SelectedElement = element;
        StateHasChanged();
    }

    public void AddElement(ElementComponent element)
    {
        if (_elementComponents.TryGetValue(element.Key, out _)) return;
        
        _elementComponents.Add(element.Key, element);
    }
}