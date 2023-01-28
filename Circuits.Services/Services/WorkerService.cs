using Accord;
using Accord.Math;
using BlazorWorker.BackgroundServiceFactory;
using BlazorWorker.Core;
using Circuits.Services.Helpers;
using Circuits.Services.Services.Interfaces;
using Circuits.Shared.Converters;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Solver;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace Circuits.Services.Services;

public class WorkerService : IWorkerService
{
    public Dictionary<EquationSystem, EquationSystemSolverState> SolverState { get; } = new();
    public Action<EquationSystem, EquationSystemSolverState>? OnUpdate { get; set; }
    public Action? OnClear { get; set; }

    private readonly IJSUtilsService _jsUtilsService;
    private readonly IJSRuntime _jsRuntime;

    private readonly IWorkerFactory _workerFactory;
    // private readonly DotNetObjectReference<WorkerService> _dotNetRef;

    public WorkerService(IJSUtilsService jsUtilsService, IJSRuntime jsRuntime, IWorkerFactory workerFactory)
    {
        _jsUtilsService = jsUtilsService;
        _jsRuntime = jsRuntime;
        _workerFactory = workerFactory;

        // _dotNetRef = DotNetObjectReference.Create(this);
    }

    public async Task RunAsync(EquationSystem equationSystem, int iterationCount = 100, float dt = 0.001f)
    {
        var scriptJs = ScriptHelper.BuildBroydensSolverJs(equationSystem, iterationCount, dt);
        var url = await _jsUtilsService.CreateObjectURLAsync(scriptJs);
        
        Console.WriteLine($"URL: {url}");
        
        
        // validate parameters ?

        if (!SolverState.TryGetValue(equationSystem, out var state))
        {
            state = new EquationSystemSolverState();
            SolverState.Add(equationSystem, state);

            foreach (var variable in equationSystem.Variables)
            {
                state.DataArrays.Add(variable, new List<float>());

                if (variable is ExpressionDerivative derivative &&
                    !state.DataArrays.ContainsKey(derivative.Variable) &&
                    !equationSystem.Variables.Contains(derivative.Variable))
                {
                    state.DataArrays.Add(derivative.Variable, new List<float>());
                }
            }
        }

        if (!(string.IsNullOrEmpty(state.Status) || state.Status == "Completed")) await StopAsync(equationSystem);

        // var scriptJs = ScriptHelper.BuildLinearSolverJs(equationSystem, iterationCount, dt);
        // var url = await _jsUtilsService.CreateObjectURLAsync(scriptJs);
        //
        // Console.WriteLine($"URL: {url}");

        // state.ScriptUrl = url;
        state.Status = "Running";
        state.DeltaTime = dt;
        foreach (var array in state.DataArrays) array.Value.Clear();

        OnUpdate?.Invoke(equationSystem, state);

        var worker = await _workerFactory.CreateAsync();
        var backgroundService = await worker.CreateBackgroundServiceAsync<SolverService>(
            options => options
                .AddAssemblyOf<SolverService>()
                .AddAssemblyOf<EquationSystem>()
                .AddAssemblyOf<TypeConverter<Expression>>()
                .AddAssemblyOf<ConvergenceException>() // Accord
                .AddAssemblyOf<MatrixType>() // Accord.Math
        );

        state.Worker = worker;
        state.BackgroundService = backgroundService;

        await backgroundService.RegisterEventListenerAsync(nameof(SolverService.CompletionEventCallback),
            (object _, string eventInfo) =>
            {
                SolverCompletedCallback(state, equationSystem);
            });
        
        await backgroundService.RegisterEventListenerAsync(nameof(SolverService.FeedbackEventCallback),
            (object _, string eventInfo) =>
            {
                var solverVariableInfos = JsonConvert.DeserializeObject<List<SolverVariableInfo>>(eventInfo);
                SolverUpdateCallback(state, equationSystem, solverVariableInfos);
            });

        var equationSystemSerialized = JsonConvert.SerializeObject(equationSystem, new TypeConverter<Expression>());
        await backgroundService.RunAsync(s => s.RunAsync(equationSystemSerialized, 100, 0.001f, 0.001f));

        Console.WriteLine("After Run");

        // await _jsRuntime.InvokeVoidAsync("startSolver", url, _dotNetRef);
    }

    public async Task StopAsync(EquationSystem equationSystem)
    {
        var state = SolverState[equationSystem];

        // if (state.Worker != null) await state.Worker.DisposeAsync();
        // if (state.BackgroundService != null) await state.BackgroundService.DisposeAsync();

        // STOP SOMEHOW
        
        state.Worker = null!;
        state.BackgroundService = null!;

        // await _jsRuntime.InvokeVoidAsync("stopSolver", state.ScriptUrl);
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

    private void SolverUpdateCallback(EquationSystemSolverState state, EquationSystem equationSystem, IList<SolverVariableInfo> solverVariableInfos)
    {
        // Console.WriteLine("SolverUpdateCallback");

        for (var i = 0; i < equationSystem.Variables.Count; i++)
        {
            var variable = equationSystem.Variables[i];
            state.DataArrays[variable].AddRange(solverVariableInfos[i].Array);

            if (variable is ExpressionDerivative derivative &&
                !equationSystem.Variables.Contains(derivative.Variable) &&
                state.DataArrays.TryGetValue(derivative.Variable, out var integralArray))
            {
                integralArray.AddRange(solverVariableInfos[i].IntegralArray);
            }
        }

        state.Status = $"{state.DataArrays.FirstOrDefault().Value.Count} / {state.IterationCount}";

        OnUpdate?.Invoke(equationSystem, state);
    }

    private void SolverCompletedCallback(EquationSystemSolverState state, EquationSystem equationSystem)
    {
        state.Status = "Completed";
        OnUpdate?.Invoke(equationSystem, state);
    }
}