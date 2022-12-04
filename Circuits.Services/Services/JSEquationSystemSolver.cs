using Circuits.Services.Services.Interfaces;

namespace Circuits.Services.Services;

public class JSEquationSystemSolver : IJSEquationSystemSolver
{
    private readonly ISchemeService _schemeService;
    
    public JSEquationSystemSolver(ISchemeService schemeService)
    {
        _schemeService = schemeService;
    }
    
    public void BuildJsFunctions()
    {
        foreach (var equationSystem in _schemeService.EquationSystems)
        {
            var index = 0;

            for (var i = equationSystem.Matrix.Length; i > -1; i--)
            {
                var function = $"function getValueFor() {{";
                
                function += "}";
                index++;
            }
        }
    }
}