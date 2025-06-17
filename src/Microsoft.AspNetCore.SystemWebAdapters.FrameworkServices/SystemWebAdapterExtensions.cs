// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using static Microsoft.AspNetCore.SystemWebAdapters.AspireConstants;

namespace System.Web;

public static partial class SystemWebAdapterExtensions
{
    public static ISystemWebAdapterBuilder AddSystemWebAdapters(this IServiceCollection services)
       => new SystemWebAdapterBuilder(services);

    public static ISystemWebAdapterBuilder AddSystemWebAdapters(this HttpApplicationHostBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var config = builder.Configuration;

        var adapters = builder.Services.AddSystemWebAdapters();

        if (config.GetValue<bool>(ProxyKeyIsEnabled))
        {
            adapters.AddProxySupport(config.GetSection(ProxyKey).Bind);
        }

        if (config.GetValue<string>(RemoteApiKey) is { })
        {
            var remoteConfig = adapters.AddRemoteAppServer(config.GetSection(RemoteKey).Bind);

            if (config.GetValue<bool>(RemoteSessionKey + IsEnabled))
            {
                remoteConfig.AddSessionServer(config.GetSection(RemoteSessionKey).Bind);
            }

            if (config.GetValue<bool>(RemoteAuthKey + IsEnabled))
            {
                remoteConfig.AddAuthenticationServer(config.GetSection(RemoteAuthKey).Bind);
            }
        }

        return adapters;
    }
}
