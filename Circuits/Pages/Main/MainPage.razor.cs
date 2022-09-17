using System.ComponentModel;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Math;
using Circuits.ViewModels.Rendering.Scheme;
using Microsoft.AspNetCore.Components;

namespace Circuits.Pages.Main;

public partial class MainPage : IDisposable
{
    [Inject] private ISchemeService _schemeService { get; set; } = null!;

    private readonly SchemeRendererContext _context = new();
    private readonly Vec2 _firstPoint = new();
    private int _mode = 0;

    private Element _selectedElement = null!;
    private Vec2 _selectedPos = new();

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

        Element element = null;

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
                    element = p1.Y < p2.Y
                        ? new Transistor { P1 = p1.Add(+2, 1), BipolarType = type }
                        : new Transistor { P1 = p1.Add(-2, -1), BipolarType = type };
                    
                    element.Rotate(p1.Y < p2.Y ? Direction.LEFT : Direction.RIGHT);
                }
                else if ((int)p1.Y == (int)p2.Y)
                {
                    element = p1.X < p2.X
                        ? new Transistor { P1 = p1.Add(1, -2), BipolarType = type }
                        : new Transistor { P1 = p1.Add(-1, +2), BipolarType = type };
                    
                    element.Rotate(p1.X < p2.X ? Direction.BOTTOM : Direction.TOP);
                }

                break;
            }
        }

        if (element is not null && !_schemeService.Intersects(element))
        {
            _schemeService.Add(element);
            _context.PencilMode = false;
        }
    }

    private void OnSelectElement(Element element)
    {
        _selectedElement = element;

        if (element != null)
        {
            _selectedPos.Set(element.TopLeft).Multiply(CellSize);
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

        // if (element is Wire wire)
        // {
        //     var testWire = GetTestWire(wire);
        //
        //     if (testWire is not null)
        //     {
        //         _schemeService.Remove(wire);
        //         
        //         wire.P1.Set(testWire.P1);
        //         wire.P2.Set(testWire.P2);
        //         
        //         _schemeService.Add(wire);
        //     }
        //
        // }
        //
        // if (element is Capacitor capacitor)
        // {
        //     var testWire = GetTestWire(capacitor);
        //
        //     if (testWire is not null)
        //     {
        //         _schemeService.Remove(capacitor);
        //         
        //         capacitor.P1 = testWire.P1;
        //         
        //         _schemeService.Add(capacitor);
        //     }
        // }
        //
        // if (element is DCSource dcSource)
        // {
        //     var testWire = GetTestWire(dcSource);
        //
        //     if (testWire is not null)
        //     {
        //         _schemeService.Remove(dcSource);
        //
        //         dcSource.Direction = (dcSource.Direction == Direction.RIGHT) ? Direction.BOTTOM : Direction.RIGHT;
        //         dcSource.P1 = testWire.P1;
        //         
        //         _schemeService.Add(dcSource);
        //     }
        // }
        
        StateHasChanged();
    }

    // private Wire GetTestWire(Element element)
    // {
    //     var isHorizontal = element.IsHorizontal();
    //     Wire newTestWire = null!;
    //
    //     if (isHorizontal)
    //     {
    //         var length = (int)(element.Points[1].X - element.Points[0].X);
    //         var halfLength = length / 2;
    //
    //         newTestWire = new Wire
    //         {
    //             P1 = new Vec2(element.Points[0].X + (halfLength == 0 ? 1 : halfLength), element.Points[0].Y - (halfLength == 0 ? 1 : halfLength)),
    //             P2 = new Vec2(element.Points[0].X + (halfLength == 0 ? 1 : halfLength), element.Points[0].Y + halfLength)
    //         };
    //     }
    //     else
    //     {
    //         var length = (int)(element.Points[1].Y - element.Points[0].Y);
    //         var halfLength = length / 2;
    //
    //         newTestWire = new Wire
    //         {
    //             P1 = new Vec2(element.Points[0].X - (halfLength == 0 ? 1 : halfLength), element.Points[0].Y + (halfLength == 0 ? 1 : halfLength)),
    //             P2 = new Vec2(element.Points[0].X + halfLength, element.Points[0].Y + (halfLength == 0 ? 1 : halfLength))
    //         };
    //     }
    //
    //     return !_schemeService.Intersects(newTestWire) ? newTestWire : null!;
    // }
}