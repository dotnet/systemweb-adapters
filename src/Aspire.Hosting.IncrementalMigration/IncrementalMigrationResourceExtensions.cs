using System.Threading.Tasks.Sources;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

using static Microsoft.AspNetCore.SystemWebAdapters.AspireConstants;

namespace Aspire.Hosting;

public static class IncrementalMigrationResourceExtensions
{
    public static IResourceBuilder<IncrementalMigration> AddIncrementalMigrationFallback<TCore, TFramework>(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<TCore> coreApp,
        IResourceBuilder<TFramework> frameworkApp,
        IResourceBuilder<ParameterResource>? apiKey = null
        )
        where TCore : IResourceWithEnvironment
        where TFramework : IResourceWithEnvironment, IResourceWithEndpoints
        => builder.AddIncrementalMigrationFallback(DefaultIncrementalServiceName, coreApp, frameworkApp, apiKey);

    public static IResourceBuilder<IncrementalMigration> AddIncrementalMigrationFallback<TCore, TFramework>(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<TCore> coreApp,
        IResourceBuilder<TFramework> frameworkApp,
        IResourceBuilder<ParameterResource>? apiKey = null
        )
        where TCore : IResourceWithEnvironment
        where TFramework : IResourceWithEnvironment, IResourceWithEndpoints
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(coreApp);
        ArgumentNullException.ThrowIfNull(frameworkApp);

        var incrementalMigration = new IncrementalMigration(name);

        apiKey ??= coreApp.ApplicationBuilder.AddParameter($"{name}-IncrementalMigration-ApiKey", () => Guid.NewGuid().ToString(), secret: true);

        coreApp.WithReferenceRelationship(frameworkApp.Resource);

        coreApp.WithEnvironment(ctx =>
        {
            ctx.EnvironmentVariables[GetKey(name, RemoteUrl)] = frameworkApp.Resource.GetEndpoint(incrementalMigration.RemoteAppEndpointName);
            ctx.EnvironmentVariables[GetKey(name, RemoteApiKey)] = apiKey;

            if (incrementalMigration.RemoteSession == RemoteSession.Enabled)
            {
                ctx.EnvironmentVariables[GetKey(name, RemoteSessionKey + IsEnabled)] = true;
            }

            if (incrementalMigration.RemoteAuthentication != RemoteAuthentication.Disabled)
            {
                ctx.EnvironmentVariables[GetKey(name, RemoteAuthKey + IsEnabled)] = true;

                if (incrementalMigration.RemoteAuthentication == RemoteAuthentication.DefaultScheme)
                {
                    ctx.EnvironmentVariables[GetKey(name, RemoteAuthIsDefaultScheme)] = true;
                }
            }
        });

        frameworkApp.WithEnvironment(ctx =>
        {
            ctx.EnvironmentVariables[GetKey(name, RemoteApiKey)] = apiKey;

            ctx.EnvironmentVariables[GetKey(name, ProxyKeyIsEnabled)] = true;

            if (incrementalMigration.RemoteSession == RemoteSession.Enabled)
            {
                ctx.EnvironmentVariables[GetKey(name, RemoteSessionKey + IsEnabled)] = true;
            }

            if (incrementalMigration.RemoteAuthentication != RemoteAuthentication.Disabled)
            {
                ctx.EnvironmentVariables[GetKey(name, RemoteAuthKey + IsEnabled)] = true;
            }
        });

        return builder.AddResource(incrementalMigration)
            .WithInitialState(new()
            {
                Properties = [],
                ResourceType = "IncrementalMigration",
                IsHidden = true,
            });
    }

    public static IResourceBuilder<IncrementalMigration> WithSession(this IResourceBuilder<IncrementalMigration> incrementalMigration, RemoteSession mode = RemoteSession.Enabled)
    {
        ArgumentNullException.ThrowIfNull(incrementalMigration);
        incrementalMigration.Resource.RemoteSession = mode;
        return incrementalMigration;
    }

    public static IResourceBuilder<IncrementalMigration> WithAuthentication(
        this IResourceBuilder<IncrementalMigration> incrementalMigration,
        RemoteAuthentication mode = RemoteAuthentication.Enabled)
    {
        ArgumentNullException.ThrowIfNull(incrementalMigration);
        incrementalMigration.Resource.RemoteAuthentication = mode;
        return incrementalMigration;
    }

    public static IResourceBuilder<IncrementalMigration> WithEndpointName(
        this IResourceBuilder<IncrementalMigration> incrementalMigration,
        string endpointName)
    {
        ArgumentNullException.ThrowIfNull(incrementalMigration);
        incrementalMigration.Resource.RemoteAppEndpointName = endpointName;
        return incrementalMigration;
    }
}
