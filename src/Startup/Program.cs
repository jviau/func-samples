using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Samples.Startup;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

 builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton<StartupEager>();

// This hosted service shows how to do asynchronous startup work.
// In this approach the initialization work is only BEGUN as part of
// worker startup. That work is then later 'joined' when it is finally needed.
// This means it is warming up in the background, but not delaying worker start.
builder.Services.AddSingleton<StartupLazy>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<StartupLazy>());

IHost host = builder.Build();

// Perform any initialization work before calling host.RunAsync
// This example uses 'StartupWork' to simulate some singleton
// which needs asynchronous initialization at startup.
StartupEager startup = host.Services.GetRequiredService<StartupEager>();
await startup.InitializeAsync();

// Functions host won't consider this worker ready for invocations until this
// RunAsync call completes.
await host.RunAsync();
