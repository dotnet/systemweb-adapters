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

    private readonly List<string> _captured = [];
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
        foreach (var existing in _captured)
        {
            output.WriteLine(existing);
        }

        return new DistributeApplicationScope(output, _app, () => _current = null);
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
            Log(str);
        }
        else
        {
            _captured.Add(str);
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
        Log("Starting initializing");

        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<TEntryPoint>();

        Log("Initialized");

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

        Log("Starting app building");

        var app = await builder.BuildAsync();
        
        Log("Starting app");

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

        sink.OnMessage(new DiagnosticMessage("All ready"));
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


    private sealed class DistributeApplicationScope : IDistributeApplicationScope
    {
        private readonly Action _dispose;
        private readonly CancellationTokenSource _cts = new();

        public DistributeApplicationScope(ITestOutputHelper output, DistributedApplication app, Action dispose)
        {
            App = app;
            _dispose = dispose;

            _ = Task.Run(async () =>
            {
                using var stdio = Console.OpenStandardOutput();
                using var reader = new StreamReader(stdio);

                while (!reader.EndOfStream && !_cts.Token.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(_cts.Token);

                    if (!string.IsNullOrEmpty(line))
                    {
                        output.WriteLine(line);
                    }
                }
            });
        }

        public DistributedApplication App { get; }

        public void Dispose()
        {
            _dispose();
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}

public interface IDistributeApplicationScope : IDisposable
{
    DistributedApplication App { get; }
}
