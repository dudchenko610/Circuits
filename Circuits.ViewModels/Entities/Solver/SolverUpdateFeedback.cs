namespace Circuits.ViewModels.Entities.Solver;

public class SolverUpdateFeedback
{
    public string Url { get; set; } = string.Empty;
    public List<SolverVariableInfo> VarInfos { get; set; } = new();
}