using System.Threading.Tasks.Sources;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

using static Microsoft.AspNetCore.SystemWebAdapters.AspireConstants;

namespace Microsoft.AspNetCore.SystemWebAdapters.Aspire;

public static class IncrementalMigrationResourceExtensions
{
    public static IResourceBuilder<TCore> WithIncrementalMigrationFallback<TCore, TFramework>(
        this IResourceBuilder<TCore> coreApp,
        IResourceBuilder<TFramework> frameworkApp,
        RemoteAuthentication remoteAuthentication = RemoteAuthentication.Disabled,
        RemoteSession remoteSession = RemoteSession.Disabled,
        IResourceBuilder<ParameterResource>? apiKey = null
        )
        where TCore : IResourceWithEnvironment
        where TFramework : IResourceWithEnvironment, IResourceWithEndpoints
    {
        ArgumentNullException.ThrowIfNull(coreApp);
        ArgumentNullException.ThrowIfNull(frameworkApp);

        apiKey ??= coreApp.ApplicationBuilder.AddParameter($"{coreApp.Resource.Name}-{frameworkApp.Resource.Name}-remoteapp-apiKey", () => Guid.NewGuid().ToString(), secret: true);

        coreApp.WithReferenceRelationship(frameworkApp.Resource);

        coreApp.WithEnvironment(ctx =>
        {
            ctx.EnvironmentVariables[RemoteUrl] = frameworkApp.Resource.GetEndpoint("https");
            ctx.EnvironmentVariables[RemoteApiKey] = apiKey;

            if (remoteSession == RemoteSession.Enabled)
            {
                ctx.EnvironmentVariables[RemoteSessionKey + IsEnabled] = true;
            }

            if (remoteAuthentication != RemoteAuthentication.Disabled)
            {
                ctx.EnvironmentVariables[RemoteAuthKey + IsEnabled] = true;

                if (remoteAuthentication == RemoteAuthentication.DefaultScheme)
                {
                    ctx.EnvironmentVariables[RemoteAuthIsDefaultScheme] = true;
                }
            }
        });

        frameworkApp.WithEnvironment(ctx =>
        {
            ctx.EnvironmentVariables[RemoteApiKey] = apiKey;

            ctx.EnvironmentVariables[ProxyKeyIsEnabled] = true;

            if (remoteSession == RemoteSession.Enabled)
            {
                ctx.EnvironmentVariables[RemoteSessionKey + IsEnabled] = true;
            }

            if (remoteAuthentication != RemoteAuthentication.Disabled)
            {
                ctx.EnvironmentVariables[RemoteAuthKey + IsEnabled] = true;
            }
        });

        return coreApp;
    }
}
