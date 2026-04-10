// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using Aspire.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public interface IAspireFixtureOptions
{
    static abstract TimeSpan InitializeTimeOut { get; }

    static abstract bool RequiresContainers { get; }
}

public sealed class ContainerAspireFixtureOptions : IAspireFixtureOptions
{
    public static TimeSpan InitializeTimeOut => TimeSpan.FromMinutes(10);

    public static bool RequiresContainers => true;
}

public sealed class DefaultAspireFixtureOptions : IAspireFixtureOptions
{
    public static TimeSpan InitializeTimeOut => TimeSpan.FromMinutes(5);

    public static bool RequiresContainers => false;
}

public class AspireFixture<TEntryPoint>(IMessageSink sink) : AspireFixture<TEntryPoint, DefaultAspireFixtureOptions>(sink), ILoggerProvider, ILogger, IAsyncLifetime
    where TEntryPoint : class
{
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
public class AspireFixture<TEntryPoint, TOptions>(IMessageSink sink) : ILoggerProvider, ILogger, IAsyncLifetime
    where TEntryPoint : class
    where TOptions : IAspireFixtureOptions
{
    private DistributedApplication? _app;
    private Exception? _startupException;

    private ImmutableList<string> _initializationLogs = [];
    private ITestOutputHelper? _current;

    public IDistributeApplicationScope GetApplicationScope(ITestOutputHelper output)
    {
        ArgumentNullException.ThrowIfNull(output);

        // flush any captured logs to the output
        foreach (var existing in _initializationLogs)
        {
            output.WriteLine(existing);
        }

        if (_current is { })
        {
            throw new InvalidOperationException("Tests cannot be run concurrently");
        }

        if (_startupException is { })
        {
            throw new InvalidOperationException("Could not start application", _startupException);
        }

        if (_app is not { })
        {
            throw new InvalidOperationException("App is not initialized");
        }

        _current = output;

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
        else
        {
            // Capture so it shows in the test logs
            ImmutableInterlocked.Update(ref _initializationLogs, list => list.Add(str));

            // Log immediately as there's a warning that the app didn't start, so we want to make sure we capture any logs that might indicate why
            if (logLevel >= LogLevel.Warning)
            {
                Log(str);
            }
        }
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        if (TOptions.RequiresContainers && !DockerStatus.IsSupported)
        {
            Log("Container support is required but not available, skipping initialization");
        }
        else
        {
            using var cts = new CancellationTokenSource(TOptions.InitializeTimeOut);

            await InitializeAsync(cts.Token);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We want to capture the exception to show up in the test output")]
    private async Task InitializeAsync(CancellationToken token)
    {
        try
        {
            _app = await BuildAppAsync(token);

            await InitializeAppAsync(_app, token);
        }
        catch (Exception ex)
        {
            if (_app is { } app)
            {
                _app = null;
                await app.DisposeAsync();
            }

            _startupException = ex;
        }
    }

    private async Task InitializeAppAsync(DistributedApplication app, CancellationToken token)
    {
        Log("Starting distributed app");

        await app.StartAsync(token);

        foreach (var resource in app.Services.GetRequiredService<DistributedApplicationModel>().Resources.Order(Comparer<IResource>.Create(ResourceComparer)))
        {
            if (resource.TryGetAnnotationsOfType<ExplicitStartupAnnotation>(out _))
            {
                continue;
            }

            Log($"Waiting for resource {resource.Name} to be ready");

            await app.ResourceNotifications.WaitForResourceHealthyAsync(resource.Name, WaitBehavior.StopOnResourceUnavailable, token);
        }

        Log("All resources ready");

        // Allows us to make sure the container resource is always first in the logs since
        // they take longer to start up
        static int ResourceComparer(IResource x, IResource y) => (x, y) switch
        {
            (ContainerResource, ContainerResource) => 0,
            (ContainerResource, _) => -1,
            (_, ContainerResource) => 1,
            (_, _) => 0
        };
    }

    private async Task<DistributedApplication> BuildAppAsync(CancellationToken token)
    {
        Log("Registering services for distributed app");

        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<TEntryPoint>(token);

        builder.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);

            // Override the logging filters from the app's configuration
            logging.AddFilter(builder.Environment.ApplicationName, LogLevel.Trace);
            logging.AddFilter("Aspire.", LogLevel.Debug);
            logging.AddFilter("Polly.", LogLevel.Error);

            logging.ClearProviders();
            logging.AddProvider(this);
        });

        // Must set the HTTP endpoint
        builder.Configuration["ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL"] = "https://localhost:21002";
        builder.Configuration["ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL"] = null;

        Log("Building distributed app");

        return await builder.BuildAsync(token);
    }

    private void Log(string message)
        => sink.OnMessage(new DiagnosticMessage("[{0}] {1}", typeof(TEntryPoint), message));

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_app is { } app)
        {
            Log("Stopping application");
            _app = null;
            await app.StopAsync();
            await app.DisposeAsync();
            Log("Stopped application");
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "<Pending>")]
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
