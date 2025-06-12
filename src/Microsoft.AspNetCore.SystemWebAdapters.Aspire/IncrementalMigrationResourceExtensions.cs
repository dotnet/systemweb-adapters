using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

using static Microsoft.AspNetCore.SystemWebAdapters.AspireConstants;

namespace Microsoft.AspNetCore.SystemWebAdapters.Aspire;

public static class IncrementalMigrationResourceExtensions
{
    public static IResourceBuilder<IncrementalMigrationResource> AddIncrementalMigration<TCore, TFramework>(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<TCore> coreApp,
        IResourceBuilder<TFramework> frameworkApp,
        IResourceBuilder<ParameterResource>? apiKey = null)
        where TCore : IResourceWithEnvironment
        where TFramework : IResourceWithEnvironment, IResourceWithEndpoints
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(coreApp);
        ArgumentNullException.ThrowIfNull(frameworkApp);

        apiKey ??= builder.AddParameter("apiKey", () => Guid.NewGuid().ToString(), secret: true);
        var resource = new IncrementalMigrationResource(name, apiKey.Resource);

        var adapter = builder.AddResource(resource)
            .WithInitialState(new()
            {
                Properties = [],
                ResourceType = "IncrementalMigrationResource",
                IsHidden = true
            });

        apiKey.WithParentRelationship(adapter);

        coreApp.WithEnvironment(ctx =>
        {
            ctx.EnvironmentVariables[RemoteUrl] = frameworkApp.Resource.GetEndpoint("https");
            ctx.EnvironmentVariables[RemoteApiKey] = apiKey;

            if (resource.IsRemoteEnabled)
            {
                ctx.EnvironmentVariables[RemoteKey + IsEnabled] = true;
            }

            if (resource.IsRemoteSessionSupported)
            {
                ctx.EnvironmentVariables[RemoteSessionKey + IsEnabled] = true;
            }

            if (resource.IsRemoteAuthenticationSupported)
            {
                ctx.EnvironmentVariables[RemoteAuthKey + IsEnabled] = true;
            }
        });

        frameworkApp.WithEnvironment(ctx =>
        {
            ctx.EnvironmentVariables[RemoteApiKey] = apiKey;

            if (resource.IsRemoteEnabled)
            {
                ctx.EnvironmentVariables[RemoteKey + IsEnabled] = true;
            }

            if (resource.IsRemoteSessionSupported)
            {
                ctx.EnvironmentVariables[RemoteSessionKey + IsEnabled] = true;
            }

            if (resource.IsRemoteAuthenticationSupported)
            {
                ctx.EnvironmentVariables[RemoteAuthKey + IsEnabled] = true;
            }
        });

        return adapter;
    }

    public static IResourceBuilder<IncrementalMigrationResource> WithRemoteAuthentication(
        this IResourceBuilder<IncrementalMigrationResource> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Resource.IsRemoteAuthenticationSupported = true;

        return builder;
    }

    public static IResourceBuilder<IncrementalMigrationResource> WithRemoteSession(
        this IResourceBuilder<IncrementalMigrationResource> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Resource.IsRemoteSessionSupported = true;

        return builder;
    }
}
