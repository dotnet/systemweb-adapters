using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

#if NET
using Microsoft.AspNetCore.Builder;
#else
using System.Web;
#endif

using static Microsoft.AspNetCore.SystemWebAdapters.AspireConstants;

namespace Microsoft.Extensions.Hosting;

public static class AspireSystemWebAdaptersExtensions
{
    public static ISystemWebAdapterBuilder AddSystemWebAdapters(this IHostApplicationBuilder builder)
    {
#if NET
        ArgumentNullException.ThrowIfNull(builder);
#else
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
#endif

        var config = builder.Configuration;
        var adapters = builder.Services.AddSystemWebAdapters();

#if NET
        builder.Services.AddReverseProxy();
#else
        if (config.GetValue<bool>(ProxyKeyIsEnabled))
        {
            adapters.AddProxySupport(config.GetSection(ProxyKey).Bind);
        }
#endif

        if (config.GetValue<string>(RemoteApiKey) is { })
        {
#if NET
            var remoteConfig = adapters.AddRemoteAppClient(config.GetSection(RemoteKey).Bind);

            if (config.GetValue<bool>(RemoteSessionKey + IsEnabled))
            {
                remoteConfig.AddSessionClient(config.GetSection(RemoteSessionKey).Bind);
            }

            if (config.GetValue<bool>(RemoteAuthKey + IsEnabled))
            {
                remoteConfig.AddAuthenticationClient(config.GetValue<bool>(RemoteAuthIsDefaultScheme), config.GetSection(RemoteAuthKey).Bind);
            }
#else
            var remoteConfig = adapters.AddRemoteAppServer(config.GetSection(RemoteKey).Bind);

            if (config.GetValue<bool>(RemoteSessionKey + IsEnabled))
            {
                remoteConfig.AddSessionServer(config.GetSection(RemoteSessionKey).Bind);
            }

            if (config.GetValue<bool>(RemoteAuthKey + IsEnabled))
            {
                remoteConfig.AddAuthenticationServer(config.GetSection(RemoteAuthKey).Bind);
            }
#endif
        }

        return adapters;
    }

#if NET
    public static IEndpointConventionBuilder MapRemoteAppFallback(this WebApplication app, [StringSyntax("Route")] string? pattern = "/{**catch-all}")
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(pattern);

        var url = app.Services.GetRequiredService<IOptions<RemoteAppClientOptions>>().Value.RemoteAppUrl.OriginalString;

        return app.MapForwarder(pattern, url)

            // If there is a route locally, we want to ensure that is used by default, but otherwise we'll forward
            .WithOrder(int.MaxValue)

            // If we're going to forward the request, there is no need to run any of the middleware after routing
            .ShortCircuit();
    }
#endif
}
