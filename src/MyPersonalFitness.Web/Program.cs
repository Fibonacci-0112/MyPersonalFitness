using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyPersonalFitness.Core;
using MyPersonalFitness.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Core fitness services with in-memory repositories (browser session storage)
builder.Services.AddMyPersonalFitnessCore();

// Browser local storage for persistence between sessions
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
