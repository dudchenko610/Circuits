namespace Circuits.Services.Services.Interfaces;

public interface IGraphService
{
    void BuildBranches();
    void BuildSpanningTree();
    void FindFundamentalCycles();
}