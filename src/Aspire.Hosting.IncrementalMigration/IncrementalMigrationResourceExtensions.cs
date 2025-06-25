using System.Threading.Tasks.Sources;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

using static Microsoft.AspNetCore.SystemWebAdapters.AspireConstants;

namespace Aspire.Hosting;

public static class IncrementalMigrationResourceExtensions
{
    public static IResourceBuilder<TCore> WithIncrementalMigrationFallback<TCore, TFramework>(
        this IResourceBuilder<TCore> coreApp,
        IResourceBuilder<TFramework> frameworkApp,
        Action<IncrementalMigrationOptions>? configureOptions = null,
        IResourceBuilder<ParameterResource>? apiKey = null
        )
        where TCore : IResourceWithEnvironment
        where TFramework : IResourceWithEnvironment, IResourceWithEndpoints
    {
        ArgumentNullException.ThrowIfNull(coreApp);
        ArgumentNullException.ThrowIfNull(frameworkApp);

        apiKey ??= coreApp.ApplicationBuilder.AddParameter($"{coreApp.Resource.Name}-{frameworkApp.Resource.Name}-remoteapp-apiKey", () => Guid.NewGuid().ToString(), secret: true);

        var options = new IncrementalMigrationOptions();
        configureOptions?.Invoke(options);

        coreApp.WithReferenceRelationship(frameworkApp.Resource);

        coreApp.WithEnvironment(ctx =>
        {
            ctx.EnvironmentVariables[RemoteUrl] = frameworkApp.Resource.GetEndpoint(options.RemoteAppEndpointName);
            ctx.EnvironmentVariables[RemoteApiKey] = apiKey;

            if (options.RemoteSession == RemoteSession.Enabled)
            {
                ctx.EnvironmentVariables[RemoteSessionKey + IsEnabled] = true;
            }

            if (options.RemoteAuthentication != RemoteAuthentication.Disabled)
            {
                ctx.EnvironmentVariables[RemoteAuthKey + IsEnabled] = true;

                if (options.RemoteAuthentication == RemoteAuthentication.DefaultScheme)
                {
                    ctx.EnvironmentVariables[RemoteAuthIsDefaultScheme] = true;
                }
            }
        });

        frameworkApp.WithEnvironment(ctx =>
        {
            ctx.EnvironmentVariables[RemoteApiKey] = apiKey;

            ctx.EnvironmentVariables[ProxyKeyIsEnabled] = true;

            if (options.RemoteSession == RemoteSession.Enabled)
            {
                ctx.EnvironmentVariables[RemoteSessionKey + IsEnabled] = true;
            }

            if (options.RemoteAuthentication != RemoteAuthentication.Disabled)
            {
                ctx.EnvironmentVariables[RemoteAuthKey + IsEnabled] = true;
            }
        });

        return coreApp;
    }
}
