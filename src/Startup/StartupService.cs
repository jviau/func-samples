using Microsoft.Extensions.Hosting;

namespace Samples.Startup;

// By using a hosted service, host.RunAsync will begin initialization.
// This is best for a "lazy" initialization. Where you want to kick work off
// as part of startup, but don't necessarily need it ready by the time the
// first function invocation comes in. But it will still be an improvement
// over initializing only when first invocation comes in.
public class StartupService : IHostedService
{
    private readonly Lazy<Task<object>> _initialization;

    public StartupService()
    {
        _initialization = new(InitializeCoreAsync);
    }

    public Task<object> GetAsync() => _initialization.Value;

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        // Do some startup work, like warming caches.
        return _initialization.Value;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    private async Task<object> InitializeCoreAsync()
    {
        // Do some init code here.
        await Task.Yield();
        return "something populated asynchronously.";
    } 
}
