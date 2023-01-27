using Circuits.ViewModels.Entities.Equations;
using Newtonsoft.Json;

namespace Circuits.Services.Services;

public class SolverService
{
    public event EventHandler<string>? EventCallback;
    
    public async Task RunAsync(string equationSystemSerialized, int iterationCount = 100, float dt = 0.001f)
    {
        Console.WriteLine("RunAsync");

        try
        {
            //var equationSystem = JsonConvert.DeserializeObject<EquationSystem>(equationSystemSerialized);

            // if (equationSystem == null)
            // {
            //     Console.WriteLine("NULL");
            //     return;
            // }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        
        Console.WriteLine("Parsed");
        
        // foreach (var variable in equationSystem.Variables)
        // {
        //     Console.WriteLine(variable.Label);
        // }
        
        for (var i = 0; i < 10; i ++)
        {
            //Console.WriteLine($"Hello from .NET worker thread {i}");
            EventCallback?.Invoke(this, $"Hello from .NET worker thread {i}");
            await Task.Delay(100);
        }
    }

    public async Task TestBroydensMethodAsync(EquationSystem equationSystem, int iterationCount = 100, float dt = 0.001f)
    {
        // Console.WriteLine("Broyden's method");
        //
        // var scriptJs = ScriptHelper.BuildBroydensSolverJs(equationSystem, iterationCount, dt);
        // var url = await _jsUtilsService.CreateObjectURLAsync(scriptJs);
        //
        // Console.WriteLine($"URL: {url}");
        //
        // await _jsRuntime.InvokeVoidAsync("testBroydensMethod", url);
        
        
        
    }
}