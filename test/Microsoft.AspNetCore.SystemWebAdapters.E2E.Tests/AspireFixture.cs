using Aspire.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public sealed class AspireFixture<TEntryPoint>(IMessageSink sink) : ILoggerProvider, ILogger, IAsyncLifetime
     where TEntryPoint : class
{
    private DistributedApplication? _app;
    private Exception? _startupException;

    private readonly List<string> _initializationLogs = [];
    private ITestOutputHelper? _current;

    public async Task<IDistributeApplicationScope> GetApplicationScopeAsync(ITestOutputHelper output)
    {
        ArgumentNullException.ThrowIfNull(output);

        if (_startupException is { })
        {
            throw new InvalidOperationException("Could not start application", _startupException);
        }

        if (_current is { })
        {
            throw new InvalidOperationException("Tests cannot be run concurrently");
        }

        if (_app is not { })
        {
            throw new InvalidOperationException("App is not initialized");
        }

        _current = output;

        // flush any captured logs to the output
        foreach (var existing in _initializationLogs)
        {
            output.WriteLine(existing);
        }

        return new DistributeApplicationScope(_app, () => _current = null);
    }

    ILogger ILoggerProvider.CreateLogger(string categoryName) => this;

    IDisposable? ILogger.BeginScope<TState>(TState state) => null;

    bool ILogger.IsEnabled(LogLevel logLevel) => true;

    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var str = $"[{logLevel}] {formatter(state, exception)}";

        if (_current is { } current)
        {
            current.WriteLine(str);
        }
        else if (_app is not { })
        {
            // Capture so it shows in the test logs
            _initializationLogs.Add(str);

            // Log immediately as there's a warning that the app didn't start, so we want to make sure we capture any logs that might indicate why
            if (logLevel >= LogLevel.Warning)
            {
                Log(str);
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    async Task IAsyncLifetime.InitializeAsync()
    {
        try
        {
            await InitializeAppAsync();
        }
        catch (Exception ex)
        {
            _startupException = ex;
        }
    }

    private async Task InitializeAppAsync()
    {
        Log("Registering services for distributed app");

        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<TEntryPoint>();

        builder.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);

            // Override the logging filters from the app's configuration
            logging.AddFilter(builder.Environment.ApplicationName, LogLevel.Trace);
            logging.AddFilter("Aspire.", LogLevel.Debug);

            logging.ClearProviders();
            logging.AddProvider(this);
        });

        // Must set the HTTP endpoint
        builder.Configuration["ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL"] = "https://localhost:21002";
        builder.Configuration["ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL"] = null;

        Log("Building distributed app");

        var app = await builder.BuildAsync();

        Log("Starting distributed app");

        await app.StartAsync();

        foreach (var resource in app.Services.GetRequiredService<DistributedApplicationModel>().Resources)
        {
            if (resource.TryGetAnnotationsOfType<ExplicitStartupAnnotation>(out _))
            {
                continue;
            }

            Log($"Waiting for resource {resource.Name} to be ready");

            await app.ResourceNotifications.WaitForResourceHealthyAsync(resource.Name, WaitBehavior.StopOnResourceUnavailable);
        }

        Log("All resources ready");

        _app = app;
    }

    private void Log(string message)
        => sink.OnMessage(new DiagnosticMessage("[{0}] {1}", typeof(TEntryPoint), message));

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_app is { } app)
        {
            _app = null;
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    void IDisposable.Dispose()
    {
    }

    private sealed class DistributeApplicationScope(DistributedApplication app, Action dispose) : IDistributeApplicationScope
    {

        public DistributedApplication App => app;

        public void Dispose() => dispose();
    }
}

public interface IDistributeApplicationScope : IDisposable
{
    DistributedApplication App { get; }
}
