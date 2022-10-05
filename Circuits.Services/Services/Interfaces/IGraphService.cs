namespace Circuits.Services.Services.Interfaces;

public interface IGraphService
{
    void BuildBranches();
    void BuildSpanningTrees();
    void FindFundamentalCycles();
}