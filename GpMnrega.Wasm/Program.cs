using GpMnrega.Wasm;
using GpMnrega.Wasm.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#wasm-root");

// HttpClient points to our own server — no CORS issues
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<INicDataService, NicDataService>();

await builder.Build().RunAsync();
