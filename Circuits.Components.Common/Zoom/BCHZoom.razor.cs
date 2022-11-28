using System.Globalization;
using Circuits.Components.Common.Models.Zoom;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Events;
using Circuits.ViewModels.Math;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Circuits.Components.Common.Zoom;

public partial class BCHZoom : IDisposable
{
    [Inject] private IJSUtilsService JsUtilsService { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    
    [Parameter] public RenderFragment<ZoomContext> ChildContent { get; set; } = null!;
    [Parameter] public ZoomContext ZoomContext { get; set; } = new();
    [Parameter] public float Factor { get; set; } = 0.009f;
    [Parameter] public float MinScale { get; set; } = 2f;
    [Parameter] public float MaxScale { get; set; } = 6.0f;
    [Parameter] public float Width { get; set; } = 1000f;
    [Parameter] public float Height { get; set; } = 1000f;
    [Parameter] public bool BoundsByParent { get; set; } = false;
    [Parameter] public bool UseConstraints { get; set; } = true;
    [Parameter] public bool ConstraintZoomByContent { get; set; } = true;
    [Parameter] public bool UseTouchRotation { get; set; } = false;
    [Parameter] public bool ZoomOnMouseWheel { get; set; } = false;
    [Parameter] public bool ShowScrollbars { get; set; } = false;
    [Parameter] public ViewMode ViewMode { get; set; } = ViewMode.Default;

    private readonly string _wrapperId = $"_id_{Guid.NewGuid()}";
    private readonly string _navigationId = $"_id_{Guid.NewGuid()}";
    private readonly NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };

    private Vec2 _size = new();
    private readonly Vec2 _viewPortSize = new();
    private readonly Vec2 _navigationSize = new();
    private Vec2 _navigationOffsetSize = new();
    private readonly Vec2 _pos = new();
    private float _scale = 4;
    private readonly Vec2 _zoomTarget = new();
    private readonly Vec2 _zoomPoint = new();
    private readonly Vec2 _lastMousePosition = new();
    private bool _dragStarted = false;
    private bool _zoomKeep = false;
    private bool _outerContentChanged = false;

    private int _touchMode = 0;
    private readonly Vec2 _pinchPos = new();
    private float _touchStartDistance = 0;
    private bool _changePerformed = false;

    private readonly Vec2 _prevRotateVec = new();
    private readonly Vec2 _rotateVec = new();
    private readonly Vec2 _touchDirToPos = new();
    private readonly Vec2 _rotatedPoint = new();
    private float _rotationAngle = 0;

    private DotNetObjectReference<BCHZoom> _dotNetObjectReference = null!;

    private float _dppx = 1.0f;

    protected override async Task OnInitializedAsync()
    {
        Width = Math.Clamp(Width, 1.0f, Width);
        Height = Math.Clamp(Height, 1.0f, Height);

        _size = new Vec2 { X = Width, Y = Height };

        IJSUtilsService.OnResize += OnResizeAsync;
        ZoomContext.ZoomUp += OnZoomUp;
        ZoomContext.ZoomDown += OnZoomDownAsync;
        
        _dppx = await JsRuntime.InvokeAsync<float>("getPixelRatio");
    }

    protected override void OnParametersSet()
    {
        MaxScale = Math.Clamp(MaxScale, 0.1f, 50f);
        MinScale = Math.Clamp(MinScale, 0.1f, 50f);

        if (MinScale >= MaxScale)
        {
            MinScale = 0.1f;
            MaxScale = 16.0f;
        }

        Factor = Math.Clamp(Factor, 0.001f, 1.0f);

        UpdateContextData();
    }

    public void Dispose()
    {
        _dotNetObjectReference?.Dispose();
        IJSUtilsService.OnResize -= OnResizeAsync;
        ZoomContext.ZoomUp -= OnZoomUp;
        ZoomContext.ZoomDown -= OnZoomDownAsync;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetObjectReference = DotNetObjectReference.Create(this);
            await JsRuntime.InvokeVoidAsync("subscribeOnMouseMove", _wrapperId, _dotNetObjectReference);
           // await CenterContentAsync();
            await OnResizeAsync();
        }

        if (_changePerformed)
        {
            _changePerformed = false;
            _outerContentChanged = false;

            var navigationRect = await JsUtilsService.GetBoundingClientRectAsync(_navigationId);
            _navigationSize.Set(navigationRect.Width, navigationRect.Height);
            _navigationOffsetSize.Set(navigationRect.OffsetWidth, navigationRect.OffsetHeight);
        }
    }

    private async Task OnResizeAsync()
    {
        var wrapperRect = await JsUtilsService.GetBoundingClientRectAsync(_wrapperId);
        var navigationRect = await JsUtilsService.GetBoundingClientRectAsync(_navigationId);

        _viewPortSize.Set(wrapperRect.Width, wrapperRect.Height);
        _navigationSize.Set(navigationRect.Width, navigationRect.Height);
        _navigationOffsetSize.Set(navigationRect.OffsetWidth, navigationRect.OffsetHeight);

        if (BoundsByParent)
        {
            _size.Set(wrapperRect.Width, wrapperRect.Height);
        }

        Update();
    }

    public async Task CenterContentAsync()
    {
        if (ViewMode == ViewMode.Default) return;
        
        var wrapperRect = await JsUtilsService.GetBoundingClientRectAsync(_wrapperId);
        var navigationRect = await JsUtilsService.GetBoundingClientRectAsync(_navigationId);
        
        _viewPortSize.Set(wrapperRect.Width, wrapperRect.Height);
        _navigationSize.Set(navigationRect.Width, navigationRect.Height);
        _navigationOffsetSize.Set(navigationRect.OffsetWidth, navigationRect.OffsetHeight);

        if (_navigationSize.X == 0 || _navigationSize.Y == 0) return;
        
        var scale = _viewPortSize.X / _navigationSize.X;
        var newHeight = _navigationSize.Y * scale;
        
        var x = 0.0f;
        var y = _viewPortSize.Y * 0.5f - newHeight * 0.5f;

        var heightCondition = ViewMode == ViewMode.StretchToBorders
            ? newHeight > _viewPortSize.Y
            : newHeight < _viewPortSize.Y;
        
        if (heightCondition)
        {
            scale = _viewPortSize.Y / _navigationSize.Y;
            var newWidth = _navigationSize.X * scale;
            
            x = _viewPortSize.X * 0.5f - newWidth * 0.5f;
            y = 0;
        }

        _pos.Set(x, y);
        _scale = (float) Math.Log(scale) + 1;
        _outerContentChanged = true;
        
        // Console.WriteLine($"wrapperWidth = {_viewPortSize.X}, navigationWidth = {_navigationSize.X}");
        // Console.WriteLine($"newHeight = {newHeight}, _viewPortSize.Y = {_viewPortSize.Y}");
        // Console.WriteLine($"_scale = {_scale}, scale = {scale}");

        UpdateContextData();
        StateHasChanged();
    }

    private async Task OnZoomDownAsync(float zoomDelta)
    {
        _zoomKeep = true;
        await OnZoomHoldAsync(zoomDelta);
    }

    private async Task OnZoomHoldAsync(float zoomDelta)
    {
        if (_zoomKeep)
        {
            Transform(_viewPortSize.X * 0.5f, _viewPortSize.Y * 0.5f, zoomDelta, 0);
            await Task.Delay(100);
            await OnZoomHoldAsync(zoomDelta);
        }
    }

    private void OnZoomUp()
    {
        _zoomKeep = false;
    }

    private void Update()
    {
        Update(_navigationSize.X, _navigationSize.Y);
    }
    
    private void Update(float predictedNavWidth, float predictedNavHeight)
    {
        if (!UseConstraints)
        {
            UpdateContextData();
            StateHasChanged();
            return;
        }

        if (predictedNavWidth < _viewPortSize.X)
        {
            if ((_pos.X + predictedNavWidth) > _size.X)
            {
                _pos.X = _size.X - predictedNavWidth;
            }
            
            if (_pos.X < 0)
            {
                _pos.X = 0;
            }
        }
        else
        {
            if ((_pos.X + predictedNavWidth) < _size.X)
            {
                _pos.X = _size.X - predictedNavWidth;
            }
        
            if (_pos.X > 0)
            {
                _pos.X = 0;
            }
        }
        
        if (predictedNavHeight < _viewPortSize.Y)
        {
            if ((_pos.Y + predictedNavHeight) > _size.Y)
            {
                _pos.Y = _size.Y - predictedNavHeight;
            }
            
            if (_pos.Y < 0)
            {
                _pos.Y = 0;
            }
        }
        else
        {
            if ((_pos.Y + predictedNavHeight) < _size.Y)
            {
                _pos.Y = _size.Y - predictedNavHeight;
            }
        
            if (_pos.Y > 0)
            {
                _pos.Y = 0;
            }
        }

        UpdateContextData();
        StateHasChanged();
    }

    private void OnMouseWheel(ExtWheelEventArgs e)
    {
        if (!ZoomOnMouseWheel) return;

        var container = e.PathCoordinates.FirstOrDefault(x => x.ClassList.Contains("navigation-cs-container"));

        if (container == null) return;

        Transform(container.X, container.Y, -(float)e.DeltaY, 0);
    }

    public void Transform(float x, float y, float zoomDelta, float angleDelta)
    {
        if (UseTouchRotation)
        {
            _touchDirToPos.Set(x, y).Subtract(_pos);
            _rotatedPoint.Set(_touchDirToPos).Rotate(angleDelta);
        
            _pos.Add(_touchDirToPos.X - _rotatedPoint.X, _touchDirToPos.Y - _rotatedPoint.Y);
            _rotationAngle += angleDelta;
        }
        
        _zoomPoint.Set(x, y);

        // determine the point on where the slide is zoomed in
        _zoomTarget.Set((_zoomPoint.X - _pos.X) / Scale, (_zoomPoint.Y - _pos.Y) / Scale);
        
        // apply zoom
        _scale += zoomDelta * Factor;
        _scale = Math.Max(MinScale, Math.Min(MaxScale, _scale));
        
        //Console.WriteLine($"ZOOM _scale = {Scale}");

        var predictedNavWidth = _navigationOffsetSize.X * Scale;
        var predictedNavHeight = _navigationOffsetSize.Y * Scale;
        
       // Console.WriteLine($"predictedNavHeight = {predictedNavHeight}");
        
        if (ConstraintZoomByContent)
        {
            var xDiff = predictedNavWidth - _viewPortSize.X;
            var yDiff = predictedNavHeight - _viewPortSize.Y;
            
            if (xDiff < 0 || yDiff < 0)
            {
                //Console.WriteLine($"ZOOM _scale = {Scale}");
                
                var newScale = xDiff < yDiff 
                    ? _viewPortSize.X / _navigationOffsetSize.X 
                    : _viewPortSize.Y / _navigationOffsetSize.Y;
                
                _scale = (float)Math.Log(newScale) + 4;
                
                //Console.WriteLine($"ZOOM newScale = {newScale} {scl} {Math.Exp(scl - 4)}");

                predictedNavWidth = _navigationOffsetSize.X * newScale;
                predictedNavHeight = _navigationOffsetSize.Y * newScale;
                //Console.WriteLine($"ZOOM  _content.H = {_navigationSize.Y} _predicted.H = {predictedNavHeight} _container.H = {_size.Y}");
            }
        }

        // calculate x and y based on zoom
        _pos.Set(
            -_zoomTarget.X * Scale + _zoomPoint.X,
            -_zoomTarget.Y * Scale + _zoomPoint.Y
        );

        _changePerformed = true;
        
        Update(predictedNavWidth, predictedNavHeight);
        //StateHasChanged();
    }

    private void OnMouseDown(MouseEventArgs e)
    {
        _lastMousePosition.Set(e.PageX, e.PageY);
        _dragStarted = true;
        
        ZoomContext.UserInteraction = true;
        ZoomContext.OnUpdate?.Invoke();
        
        StateHasChanged();
    }

    private void OnMouseLeaveUp()
    {
        _dragStarted = false;
        _touchMode = 0;
        _prevRotateVec.Set(0, 0);

        ZoomContext.UserInteraction = false;
        ZoomContext.OnUpdate?.Invoke();
        
        StateHasChanged();
    }

    [JSInvokable]
    public void OnMouseMove(ExtMouseEventArgs e)
    {
        if (!_dragStarted) return;

        var mousePosition = new Vec2(e.PageX, e.PageY);
        var change = new Vec2(
            mousePosition.X - _lastMousePosition.X,
            mousePosition.Y - _lastMousePosition.Y
        );

        _lastMousePosition.Set(mousePosition);
        _pos.Add(change);
        
        _changePerformed = true;

        Update();
    }

    private async Task OnTouchStartAsync(TouchEventArgs e)
    {
        if (e.Touches.Length == 1)
        {
            _touchMode = 1;
            OnMouseDown(new MouseEventArgs
            {
                PageX = e.Touches[0].PageX,
                PageY = e.Touches[0].PageY
            });
            
            ZoomContext.UserInteraction = true;
            ZoomContext.OnUpdate?.Invoke();

            return;
        }

        if (e.Touches.Length == 2)
        {
            _touchMode = 2;

            var dx = e.Touches[0].ScreenX - e.Touches[1].ScreenX;
            var dy = e.Touches[0].ScreenY - e.Touches[1].ScreenY;

            var wrapperRect = await JsUtilsService.GetBoundingClientRectAsync(_wrapperId);

            var x0 = e.Touches[0].PageX - wrapperRect.Left;
            var y0 = e.Touches[0].PageY - wrapperRect.Top;
            
            var x1 = e.Touches[1].PageX - wrapperRect.Left;
            var y1 = e.Touches[1].PageY - wrapperRect.Top;

            _pinchPos.Set(
                x0 + (x1 - x0) * 0.5f,
                y0 + (y1 - y0) * 0.5f);

            _touchStartDistance = (float)Math.Sqrt(dx * dx + dy * dy);
            
            ZoomContext.UserInteraction = true;
            ZoomContext.OnUpdate?.Invoke();
        }
    }

    private void OnTouchMove(TouchEventArgs e)
    {
        if (e.Touches.Length == 1 && _touchMode == 1)
        {
            OnMouseMove(new ExtMouseEventArgs
            {
                PageX = e.Touches[0].PageX,
                PageY = e.Touches[0].PageY
            });

            return;
        }

        if (e.Touches.Length == 2 && _touchMode == 2)
        {
            var dx = e.Touches[0].ScreenX - e.Touches[1].ScreenX;
            var dy = e.Touches[0].ScreenY - e.Touches[1].ScreenY;

            var touchDist2 = (float) Math.Sqrt((dx * dx) + (dy * dy));
            var deltaScale = touchDist2 - _touchStartDistance;

            _touchStartDistance = touchDist2;
            
            _rotateVec.Set(dx, dy);

            var deltaAngle = 0f;

            if (_prevRotateVec is not { X: 0, Y: 0 })
            {
                var dot = Vec2.DotProduct(_rotateVec, _prevRotateVec);
                var absA = _rotateVec.Length();
                var absB = _prevRotateVec.Length();

                var cos = dot / (absA * absB);
                var angleRadians = Math.Acos(cos);

                var cross = Vec2.CrossProduct(_rotateVec, _prevRotateVec);
                
                deltaAngle = (float) ((cross < 0 ? 1 : -1) * angleRadians);

                if (double.IsNaN(deltaAngle)) deltaAngle = 0;
            }

            _prevRotateVec.Set(_rotateVec);
            Transform(_pinchPos.X, _pinchPos.Y, deltaScale / _dppx, deltaAngle);

            return;
        }

        _touchMode = 0;
        _prevRotateVec.Set(0, 0);
    }

    private void OnTouchEnd(TouchEventArgs e)
    {
        OnMouseLeaveUp();
    }

    private void UpdateContextData()
    {
        var prevScale = ZoomContext.ScaleLinear;
        var prevAngle = ZoomContext.AngleInRadians;
        var prevX = ZoomContext.TopLeftPos.X;
        var prevY = ZoomContext.TopLeftPos.Y;
        
        ZoomContext.TopLeftPos.Set(_pos);
        ZoomContext.Scale = Scale;
        ZoomContext.ScaleLinear = _scale;
        ZoomContext.AngleInRadians = _rotationAngle;
        
        if (prevScale != ZoomContext.ScaleLinear || 
            prevAngle != ZoomContext.AngleInRadians ||
            prevX != ZoomContext.TopLeftPos.X ||
            prevY != ZoomContext.TopLeftPos.Y)
            ZoomContext.OnUpdate?.Invoke();
    }

    private float Scale => (float)Math.Exp(_scale - 4);

    private string GetNavigationStyle()
    {
        return ($"width: {_size.X.ToString(_nF)}px; height: {_size.Y.ToString(_nF)}px; transform:" +
                $"translate({_pos.X.ToString(_nF)}px, {_pos.Y.ToString(_nF)}px) " +
                $"scale({Scale.ToString(_nF)}) " +
                $"rotate({(_rotationAngle * (180 / Math.PI)).ToString(_nF)}deg); " +
                $"{(true ? "transition: transform 0s;" : "")}"); // cursor: move;
                // $"{(_dragStarted || _touchMode == 2 || _outerContentChanged ? "transition: transform 0s; cursor: move;" : "")}");
    }
}