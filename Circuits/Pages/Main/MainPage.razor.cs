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
    
    private void OnPenClicked()
    {
        _context.PencilMode = !_context.PencilMode;
        _mode = 0;
    }
    
    private void OnResistorClicked()
    {
        _context.PencilMode = !_context.PencilMode;
        _mode = 1;
    }

    private void OnFirstPointSet(Vec2 firstPoint)
    {
        _firstPoint.Set(firstPoint);
    }
    
    private void OnSecondPointSet(Vec2 secondPoint)
    {
        var p1 = new Vec2((int)_firstPoint.X / CellSize, (int)_firstPoint.Y / CellSize);
        var p2 = new Vec2((int)secondPoint.X / CellSize, (int)secondPoint.Y / CellSize);

        switch (_mode)
        {
            case 0:
            {
                Wire wire = null!;
        
                if ((int) p1.X == (int) p2.X)
                {
                    wire = p1.Y < p2.Y ? new Wire { P1 = p1, P2 = p2 } : new Wire { P1 = p2, P2 = p1 };
                }
                else if ((int) p1.Y == (int) p2.Y)
                {
                    wire = p1.X < p2.X ? new Wire { P1 = p1, P2 = p2 } : new Wire { P1 = p2, P2 = p1 };
                }
        
                if (wire is not null && !_schemeService.Intersects(wire))
                {
                    _schemeService.Add(wire);
                    _context.PencilMode = false;
                }
                
                break;
            }
            case 1:
            {
                Resistor resistor = null!;
                
                if ((int) p1.X == (int) p2.X)
                {
                    resistor = p1.Y < p2.Y ? 
                        new Resistor { Direction = Direction.BOTTOM, P1 = p1 } : 
                        new Resistor { Direction = Direction.BOTTOM, P1 = p1.Add(0, -2) };
                }
                else if ((int) p1.Y == (int) p2.Y)
                {
                    resistor = p1.X < p2.X ? 
                        new Resistor { Direction = Direction.RIGHT, P1 = p1 } : 
                        new Resistor { Direction = Direction.RIGHT, P1 = p1.Add(-2, 0) };
                }
                
                if (resistor != null && !_schemeService.Intersects(resistor))
                {
                    _schemeService.Add(resistor);
                    _context.PencilMode = false;
                }
                
                break;
            }
        }
        

    }
}
