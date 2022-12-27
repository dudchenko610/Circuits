using Circuits.Services.Helpers;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Solver;
using Microsoft.JSInterop;

namespace Circuits.Services.Services;

public class SolverService : ISolverService
{
    public Dictionary<EquationSystem, EquationSystemSolverState> SolverState { get; } = new();
    public Action<EquationSystem, EquationSystemSolverState>? OnUpdate { get; set; }
    public Action? OnClear { get; set; } 
    
    private readonly IJSUtilsService _jsUtilsService;
    private readonly IJSRuntime _jsRuntime;
    private readonly DotNetObjectReference<SolverService> _dotNetRef;
    
    public SolverService(IJSUtilsService jsUtilsService, IJSRuntime jsRuntime)
    {
        _jsUtilsService = jsUtilsService;
        _jsRuntime = jsRuntime;

        _dotNetRef = DotNetObjectReference.Create(this);
    }
    
    public async Task RunAsync(EquationSystem equationSystem)
    {
        if (!SolverState.TryGetValue(equationSystem, out var state))
        {
            state = new EquationSystemSolverState();
            SolverState.Add(equationSystem, state);
            
            foreach (var variable in equationSystem.Variables)
            {
                state.DataArrays.Add(variable, new List<float>());
            }
        }

        if (!(string.IsNullOrEmpty(state.Status) || state.Status == "Completed")) await StopAsync(equationSystem);
        
        
        var scriptJs = ScriptHelper.BuildCalculatingJs(equationSystem, 100, 0.001f);
        var url = await _jsUtilsService.CreateObjectURLAsync(scriptJs);

        Console.WriteLine($"URL: {url}");

        state.ScriptUrl = url;
        state.Status = "Running";

        foreach (var array in state.DataArrays)
        {
            array.Value.Clear();
        }
        
        await _jsRuntime.InvokeVoidAsync("startSolver", url, _dotNetRef);
        
        OnUpdate?.Invoke(equationSystem, state);
        
        // var jsObject = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", url);
        // await jsObject.InvokeVoidAsync("testIntegration");
        // await jsObject.DisposeAsync();
        // await _jsUtilsService.RevokeObjectAsync(url);
    }

    public async Task StopAsync(EquationSystem equationSystem)
    {
        var state = SolverState[equationSystem];
        
        await _jsRuntime.InvokeVoidAsync("stopSolver", state.ScriptUrl);
        state.Status = "Completed";

        OnUpdate?.Invoke(equationSystem, state);
    }

    public async Task ClearAsync()
    {
        foreach (var keyValue in SolverState)
        {
            await StopAsync(keyValue.Key);
        }
        
        SolverState.Clear();
        OnClear?.Invoke();
    }

    [JSInvokable]
    public void SolverUpdateCallback(SolverUpdateFeedback feedback)
    {
        var (equationSystem, state) = SolverState.FirstOrDefault(x => x.Value.ScriptUrl == feedback.Url);
        if (state is null) throw new Exception("Solver state is broken");

        Console.WriteLine("SolverUpdateCallback");
        
        for (var i = 0; i < equationSystem.Variables.Count; i ++)
        {
            var variable = equationSystem.Variables[i];
            state.DataArrays[variable].AddRange(feedback.DataArrays[i]);
        }

        state.Status = $"{state.DataArrays.FirstOrDefault().Value.Count} / {state.IterationCount}";
        
        OnUpdate?.Invoke(equationSystem, state);
    }

    [JSInvokable]
    public void SolverCompletedCallback(string url)
    {
        var (equationSystem, state) = SolverState.FirstOrDefault(x => x.Value.ScriptUrl == url);
        if (state is null) throw new Exception("Solver state is broken");
        
        Console.WriteLine("SolverCompletedCallback");
        
        state.Status = "Completed";
        OnUpdate?.Invoke(equationSystem, state);
    }
}