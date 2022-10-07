using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Math;
using Circuits.ViewModels.Rendering;
using Circuits.ViewModels.Rendering.Scheme;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace Circuits.Pages.Main;

public partial class MainPage : IDisposable
{
    [Inject] private IElementService _schemeService { get; set; } = null!;

    private NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    private readonly NavigationPlaneContext _navPlaneContext = new();
    private readonly SchemeRendererContext _context = new();
    private readonly Vec2 _firstPoint = new();
    private int _mode = 0;

    private Element _selectedElement = null!;
    private readonly Vec2 _selectedPos = new();

    private bool _showGraphInspector = false;

    private static int CellSize => SchemeRendererContext.CellSize;

    protected override void OnInitialized()
    {
        _context.OnUpdate += StateHasChanged;
    }

    public void Dispose()
    {
        _context.OnUpdate -= StateHasChanged;
    }

    private void OnPenClicked(int mode)
    {
        if (!_context.PencilMode)
        {
            _context.PencilMode = true;
        }
        else if (mode == _mode)
        {
            _context.PencilMode = false;
        }

        _mode = mode;
    }

    private void OnFirstPointSet(Vec2 firstPoint)
    {
        _firstPoint.Set(firstPoint);
    }

    private void OnSecondPointSet(Vec2 secondPoint)
    {
        var p1 = new Vec2((int)_firstPoint.X / CellSize, (int)_firstPoint.Y / CellSize);
        var p2 = new Vec2((int)secondPoint.X / CellSize, (int)secondPoint.Y / CellSize);

        Element element = null!;

        switch (_mode)
        {
            case 0:
            {
                if ((int)p1.X == (int)p2.X)
                {
                    element = p1.Y < p2.Y ? new Wire(p1, p2) : new Wire(p2, p1);
                }
                else if ((int)p1.Y == (int)p2.Y)
                {
                    element = p1.X < p2.X ? new Wire(p1, p2) : new Wire(p2, p1);
                }
                break;
            }
            case 1:
            {
                if ((int)p1.X == (int)p2.X)
                {
                    element = p1.Y < p2.Y
                        ? new Resistor(p1, Direction.TOP)
                        : new Resistor(p1.Add(0,-2), Direction.BOTTOM);
                }
                else if ((int)p1.Y == (int)p2.Y)
                {
                    element = p1.X < p2.X
                        ? new Resistor(p1, Direction.LEFT)
                        : new Resistor(p1.Add(-2,0), Direction.RIGHT);
                }

                break;
            }
            case 2:
            {
                if ((int)p1.X == (int)p2.X)
                {
                    element = p1.Y < p2.Y
                        ? new Capacitor(p1, Direction.TOP)
                        : new Capacitor(p1.Add(0,-2), Direction.BOTTOM);
                }
                else if ((int)p1.Y == (int)p2.Y)
                {
                    element = p1.X < p2.X
                        ? new Capacitor(p1, Direction.LEFT)
                        : new Capacitor(p1.Add(-2,0), Direction.RIGHT);
                }

                break;
            }
            case 3:
            {
                if ((int)p1.X == (int)p2.X)
                {
                    element = p1.Y < p2.Y
                        ? new Inductor(p1, Direction.TOP)
                        : new Inductor(p1.Add(0,-2), Direction.BOTTOM);
                }
                else if ((int)p1.Y == (int)p2.Y)
                {
                    element = p1.X < p2.X
                        ? new Inductor(p1, Direction.LEFT)
                        : new Inductor(p1.Add(-2,0), Direction.RIGHT);
                }

                break;
            }
            case 4:
            {
                if ((int)p1.X == (int)p2.X)
                {
                    element = p1.Y < p2.Y
                        ? new DCSource(p1, Direction.TOP)
                        : new DCSource(p1.Add(0,-2), Direction.BOTTOM);
                }
                else if ((int)p1.Y == (int)p2.Y)
                {
                    element = p1.X < p2.X
                        ? new DCSource(p1, Direction.LEFT)
                        : new DCSource(p1.Add(-2,0), Direction.RIGHT);
                }

                break;
            }
            case 5:
            case 6:
            {
                var type = _mode == 5 ? BipolarTransistorType.PNP : BipolarTransistorType.NPN;

                if ((int)p1.X == (int)p2.X)
                {
                    var condition = p1.Y < p2.Y;
                    
                    element = condition
                        ? new Transistor { P1 = p1.Add(+2, 1), BipolarType = type }
                        : new Transistor { P1 = p1.Add(-2, -1), BipolarType = type };
                    
                    element.Rotate(condition ? Direction.LEFT : Direction.RIGHT);
                }
                else if ((int)p1.Y == (int)p2.Y)
                {
                    var condition = p1.X < p2.X;
                    
                    element = condition
                        ? new Transistor { P1 = p1.Add(1, -2), BipolarType = type }
                        : new Transistor { P1 = p1.Add(-1, +2), BipolarType = type };
                    
                    element.Rotate(condition ? Direction.BOTTOM : Direction.TOP);
                }

                break;
            }
        }

        if (element != null! && !_schemeService.Intersects(element))
        {
            _schemeService.Add(element);
            //_context.PencilMode = false;
        }
    }

    private void OnSelectElement(Element element)
    {
        _selectedElement = element;

        if (element != null!)
        {
            _selectedPos.Set(
                element.TopLeft.X * CellSize * _navPlaneContext.Scale + _navPlaneContext.TopLeftPos.X, 
                element.TopLeft.Y * CellSize * _navPlaneContext.Scale + _navPlaneContext.TopLeftPos.Y);
        }

        StateHasChanged();
    }

    private void RemoveElement(Element element)
    {
        _schemeService.Remove(element);
        StateHasChanged();
    }

    private void RotateElement(Element element)
    {
        _schemeService.Remove(element);
        element.Rotate((Direction) (((int) element.Direction + 1) % 4));
        
        _schemeService.Add(element);

        StateHasChanged();
    }
}