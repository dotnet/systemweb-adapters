using Aspire.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public sealed class AspireFixture<TEntryPoint> : IDisposable, IAsyncDisposable, ILoggerProvider, ILogger
     where TEntryPoint : class
{
    private readonly Task<DistributedApplication> _app;

    public AspireFixture(IMessageSink sink)
    {
        _app = Task.Run(async () =>
        {
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

            var app = await builder.BuildAsync();

            sink.OnMessage(new DiagnosticMessage($"Starting application {typeof(TEntryPoint)}"));
            await app.StartAsync();
            sink.OnMessage(new DiagnosticMessage($"Application {typeof(TEntryPoint)} started"));

            return app;
        });
    }

    public void Dispose()
    {
        _app.Result.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await (await _app).DisposeAsync();
    }

    private ITestOutputHelper? _current;

    public async Task<IDistributeApplicationScope> GetApplicationScopeAsync(ITestOutputHelper output)
    {
        if (_current is { })
        {
            throw new InvalidOperationException("Tests cannot be run concurrently");
        }

        var app = await _app;
        _current = output;

        return new DistributeApplicationScope(app, () => _current = null);
    }

    ILogger ILoggerProvider.CreateLogger(string categoryName) => this;

    IDisposable? ILogger.BeginScope<TState>(TState state) => null;

    bool ILogger.IsEnabled(LogLevel logLevel) => true;

    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (_current is { } current)
        {
            current.WriteLine($"[{logLevel}] {formatter(state, exception)}");
        }
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
