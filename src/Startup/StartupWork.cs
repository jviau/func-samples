namespace Samples.Startup;

public class StartupWork
{
    private readonly Lazy<Task> _initialization;
    private object? _value;

    public StartupWork()
    {
        _initialization = new(InitializeCoreAsync);
    }

    public object Value =>
        _value is null ? throw new InvalidOperationException("Call InitializeAsync first.") : _value;

    public Task InitializeAsync() => _initialization.Value;

    private Task InitializeCoreAsync()
    {
        // Do some init code here.
        _value = "something populated asynchronously.";
        return Task.CompletedTask;
    }
}
