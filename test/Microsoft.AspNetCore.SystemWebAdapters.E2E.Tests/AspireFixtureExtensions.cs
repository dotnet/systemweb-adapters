using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Azure;
using Microsoft.Extensions.Hosting;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

internal static class AspireFixtureExtensions
{
    public static IResource GetResource(this DistributedApplication app, string resourceName)
    {
        ThrowIfNotStarted(app);

        var applicationModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var resources = applicationModel.Resources;
        var resource = resources.SingleOrDefault(r => string.Equals(r.Name, resourceName, StringComparison.OrdinalIgnoreCase));

        ArgumentNullException.ThrowIfNull(resource);

        return resource;
    }

    public static async Task<IEnumerable<KeyValuePair<string, string>>> GetIncrementalMigrationEnvironmentVariableValuesAsync<TEntryPoint>(this AspireFixture<TEntryPoint> fixture, string resourceName, ITestOutputHelper output)
        where TEntryPoint : class
    {
        var app = await fixture.GetApplicationAsync();
        var resource = Assert.IsAssignableFrom<IResourceWithEnvironment>(app.GetResource(resourceName));
        var values = await resource.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Publish);
        var filtered = values.Where(s => s.Key.StartsWith("IncrementalMigration__", StringComparison.Ordinal)).OrderBy(s => s.Key).ToList();

        output.WriteLine($"Environment variables for resource '{resourceName}':");

        foreach (var kvp in filtered)
        {
            output.WriteLine($"{kvp.Key} = {kvp.Value}");
        }

        output.WriteLine("---------------------------------");

        return filtered;
    }

    static void ThrowIfNotStarted(DistributedApplication app)
    {
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        if (!lifetime.ApplicationStarted.IsCancellationRequested)
        {
            throw new InvalidOperationException("App not started");
        }
    }
}
