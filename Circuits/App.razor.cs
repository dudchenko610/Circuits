using Circuits.Services.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Circuits;

public partial class App
{
    [Inject] private ISchemeService SchemeService { get; set; } = null!;
    [Inject] public IStorageService StorageService { get; set; } = null!;
    [Inject] private IGraphService GraphService { get; set; } = null!;
    [Inject] private IElectricalSystemService ElectricalSystemService { get; set; } = null!;
    
    protected override async Task OnInitializedAsync()
    {
        await StorageService.RestoreAsync();
        
        GraphService.BuildBranches();
        GraphService.BuildSpanningTrees();
        GraphService.FindFundamentalCycles();
        GraphService.CollectProperties();
        
    //    ElectricalSystemService.BuildEquationSystemsFromGraphs(SchemeService.Graphs);
    }
}