using System;
using System.Globalization;
using Circuits.ViewModels.Events;
using Circuits.ViewModels.Math;
using Circuits.ViewModels.Rendering;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeRenderer;

class Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

class Wire
{
    public Point P1 { get; set; } = new();
    public Point P2 { get; set; } = new();
}

public partial class SchemeRendererComponent : IDisposable
{
    [Parameter] public SchemeRendererContext SchemeRendererContext { get; set; } = null!;
    [Parameter] public float Scale { get; set; } = 1.0f;

    private NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    private Vec2 _wirePointerPos = new();
    private const int CellSize = 30;

    private Vec2 _firstPointPos = new();
    private bool _firstPointSet = false;
    private List<Wire> _wires = new();
    private Wire _selectedWire = null!;

    protected override void OnInitialized()
    {
        SchemeRendererContext.OnUpdate += Update;
    }

    public void Dispose()
    {
        SchemeRendererContext.OnUpdate -= Update;
    }

    private void Update()
    {
        if (!SchemeRendererContext.PencilMode)
        {
            _firstPointSet = false;
        }

        StateHasChanged();
    }

    private void OnWireClicked(Wire wire)
    {
        _selectedWire = wire;
        StateHasChanged();
    }
    
    private void OnMouseMove(ExtMouseEventArgs e)
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

            if (_wirePointerPos.X != posX || _wirePointerPos.Y != posY)
            {
                _wirePointerPos.Set(posX, posY);
                
                StateHasChanged();
            }
        }
    }

    private void OnContainerDown()
    {
        OnFirstPointSet();
    }

    private void OnContainerUp()
    {
        OnSecondPointSet();
    }

    private void OnContainerClicked()
    {
        // if (OnFirstPointSet())
        //     return;
        //
        // if(OnSecondPointSet())
        //     return;

        if(_selectedWire != null!)
        {
            _selectedWire = null!;
            StateHasChanged();
        }
    }

    private bool OnFirstPointSet()
    {
        if (SchemeRendererContext.PencilMode && !_firstPointSet)
        {
            _firstPointSet = true;
            _firstPointPos.Set(_wirePointerPos);
            
            StateHasChanged();

            return true;
        }
        
        return false;
    }

    private bool OnSecondPointSet()
    {
        if (SchemeRendererContext.PencilMode && _firstPointSet)
        {
            var p1 = new Point()
            {
                X = (int)_firstPointPos.X / CellSize,
                Y = (int)_firstPointPos.Y / CellSize
            };

            var p2 = new Point()
            {
                X = (int)_wirePointerPos.X / CellSize,
                Y = (int)_wirePointerPos.Y / CellSize
            };

            if (p1.X == p2.X && p1.Y != p2.Y)
            {
                if (p1.Y < p2.Y)
                {
                    _wires.Add(new Wire { P1 = p1, P2 = p2 });
                }
                else
                {
                    _wires.Add(new Wire { P1 = p2, P2 = p1 });
                }
            }
            else if (p1.Y == p2.Y && p1.X != p2.X)
            {
                if (p1.X < p2.X)
                {
                    _wires.Add(new Wire { P1 = p1, P2 = p2 });
                }
                else
                {
                    _wires.Add(new Wire { P1 = p2, P2 = p1 });
                }
            }

            SchemeRendererContext.PencilMode = false;
            
            return true;
        }

        return false;
    }
}