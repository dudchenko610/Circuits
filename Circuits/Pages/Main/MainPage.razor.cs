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
        _context.PencilMode = !_context.PencilMode;
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
                if ((int) p1.X == (int) p2.X)
                {
                    element = p1.Y < p2.Y ? new Wire { P1 = p1, P2 = p2 } : new Wire { P1 = p2, P2 = p1 };
                }
                else if ((int) p1.Y == (int) p2.Y)
                {
                    element = p1.X < p2.X ? new Wire { P1 = p1, P2 = p2 } : new Wire { P1 = p2, P2 = p1 };
                }

                break;
            }
            case 1:
            {
                if ((int) p1.X == (int) p2.X)
                {
                    element = p1.Y < p2.Y ? 
                        new Resistor { Direction = Direction.BOTTOM, P1 = p1 } : 
                        new Resistor { Direction = Direction.BOTTOM, P1 = p1.Add(0, -2) };
                }
                else if ((int) p1.Y == (int) p2.Y)
                {
                    element = p1.X < p2.X ? 
                        new Resistor { Direction = Direction.RIGHT, P1 = p1 } : 
                        new Resistor { Direction = Direction.RIGHT, P1 = p1.Add(-2, 0) };
                }
 
                break;
            }
            case 2:
            {
                if ((int) p1.X == (int) p2.X)
                {
                    element = p1.Y < p2.Y ? 
                        new Capacitor { Direction = Direction.BOTTOM, P1 = p1 } : 
                        new Capacitor { Direction = Direction.BOTTOM, P1 = p1.Add(0, -2) };
                }
                else if ((int) p1.Y == (int) p2.Y)
                {
                    element = p1.X < p2.X ? 
                        new Capacitor { Direction = Direction.RIGHT, P1 = p1 } : 
                        new Capacitor { Direction = Direction.RIGHT, P1 = p1.Add(-2, 0) };
                }

                break;
            }
            case 3:
            {
                if ((int) p1.X == (int) p2.X)
                {
                    element = p1.Y < p2.Y ? 
                        new Inductor() { Direction = Direction.BOTTOM, P1 = p1 } : 
                        new Inductor { Direction = Direction.BOTTOM, P1 = p1.Add(0, -2) };
                }
                else if ((int) p1.Y == (int) p2.Y)
                {
                    element = p1.X < p2.X ? 
                        new Inductor { Direction = Direction.RIGHT, P1 = p1 } : 
                        new Inductor { Direction = Direction.RIGHT, P1 = p1.Add(-2, 0) };
                }

                break;
            }
            case 4:
            {
                if ((int) p1.X == (int) p2.X)
                {
                    element = p1.Y < p2.Y ? 
                        new DCSource() { Direction = Direction.BOTTOM, P1 = p1 } : 
                        new DCSource { Direction = Direction.BOTTOM, P1 = p1.Add(0, -2), Reverse = true };
                }
                else if ((int) p1.Y == (int) p2.Y)
                {
                    element = p1.X < p2.X ? 
                        new DCSource { Direction = Direction.RIGHT, P1 = p1 } : 
                        new DCSource { Direction = Direction.RIGHT, P1 = p1.Add(-2, 0), Reverse = true  };
                }

                break;
            }            
            case 5:
            {
                if ((int) p1.X == (int) p2.X)
                {
                    element = p1.Y < p2.Y ? 
                        new Transistor { P1 = p1 } : 
                        new Transistor { P1 = p1.Add(0, -2)};
                }
                else if ((int) p1.Y == (int) p2.Y)
                {
                    element = p1.X < p2.X ? 
                        new Transistor { P1 = p1 } : 
                        new Transistor { P1 = p1.Add(-2, 0) };
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
}
