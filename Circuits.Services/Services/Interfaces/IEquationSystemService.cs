using Circuits.ViewModels.Entities.Equations;

namespace Circuits.Services.Services.Interfaces;

public interface IEquationSystemService
{
    void PerformKirchhoffElimination(EquationSystem system);
}