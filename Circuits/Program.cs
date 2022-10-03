using Circuits;
using Circuits.Services.Services;
using Circuits.Services.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var services = builder.Services;

services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
services.AddScoped<IJSUtilsService, JSUtilsService>();
services.AddScoped<IElementService, ElementService>();
services.AddScoped<IGraphService, GraphService>();
services.AddScoped<IHighlightService, HighlightService>();
services.AddScoped<ISchemeService, SchemeService>();

await builder.Build().RunAsync();
