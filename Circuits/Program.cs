using Circuits;
using Circuits.Components.Common.Modal.Root;
using Circuits.Components.Common.Services;
using Circuits.Components.Common.Services.Interfaces;
using Circuits.Services.Database;
using Circuits.Services.Services;
using Circuits.Services.Services.Interfaces;
using DnetIndexedDb;
using DnetIndexedDb.Fluent;
using DnetIndexedDb.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.RootComponents.Add<BCHRootModal>("body::after");

var services = builder.Services;

services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
services.AddScoped<IJSUtilsService, JSUtilsService>();
services.AddScoped<IPopupService, PopupService>();
services.AddScoped<IElementService, ElementService>();
services.AddScoped<IGraphService, GraphService>();
services.AddScoped<IHighlightService, HighlightService>();
services.AddScoped<ISchemeService, SchemeService>();
services.AddScoped<IEquationSystemService, EquationSystemService>();
services.AddScoped<IElectricalSystemService, ElectricalSystemService>();
services.AddScoped<IStorageService, StorageService>();
services.AddScoped<IJSEquationSystemSolver, JSEquationSystemSolver>();

services.AddIndexedDbDatabase<DataIndexedDb>(options =>
{
    var dbModel = new IndexedDbDatabaseModel()
        .WithName("SchemeElements")
        .WithVersion(1)
        .WithModelId(0);

    dbModel.AddStore("Wire")
        .WithAutoIncrementingKey("Id")
        .AddUniqueIndex("Id");
    dbModel.AddStore("Transistor")   
        .WithAutoIncrementingKey("Id")
        .AddUniqueIndex("Id");
    dbModel.AddStore("Resistor")   
        .WithAutoIncrementingKey("Id")
        .AddUniqueIndex("Id");
    dbModel.AddStore("Inductor")   
        .WithAutoIncrementingKey("Id")
        .AddUniqueIndex("Id");
    dbModel.AddStore("DCSource")   
        .WithAutoIncrementingKey("Id")
        .AddUniqueIndex("Id");
    dbModel.AddStore("Capacitor")   
        .WithAutoIncrementingKey("Id")
        .AddUniqueIndex("Id");

    options.UseDatabase(dbModel);
});

await builder.Build().RunAsync();
