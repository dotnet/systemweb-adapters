using Aspire.Hosting;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public sealed class AspireFixture<TEntryPoint> : IDisposable, IAsyncDisposable
     where TEntryPoint : class
{
    private readonly Task<DistributedApplication> _app;

    public AspireFixture()
    {
        _app = Task.Run(async () =>
        {
            var builder = await DistributedApplicationTestingBuilder
                .CreateAsync<TEntryPoint>();

            var app = await builder.BuildAsync();

            await app.StartAsync();

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

    public Task<DistributedApplication> GetApplicationAsync() => _app;
}
