using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Structures;

namespace Circuits.Services.Services.Interfaces;

public interface IElectricalSystemService
{
    List<EquationSystem> BuildEquationSystemsFromGraphs(IEnumerable<Graph> graphs);
}