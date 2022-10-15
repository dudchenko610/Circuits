using Circuits.ViewModels.Entities.Equations;

namespace Circuits.Services.Services.Interfaces;

public interface IEquationSystemService
{
    string PerformKirchhoffElimination(EquationSystem equationSystem);
}