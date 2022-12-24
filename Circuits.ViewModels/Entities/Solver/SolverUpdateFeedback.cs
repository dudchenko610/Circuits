namespace Circuits.ViewModels.Entities.Solver;

public class SolverUpdateFeedback
{
    public string Url { get; set; } = string.Empty;
    public List<List<float>> DataArrays { get; set; } = new();
}