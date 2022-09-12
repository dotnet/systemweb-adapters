// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class RemoteAppClientExtensions
{
    /// <summary>
    /// Add configuration for connecting to a remote app for System.Web extensions that require a remote
    /// ASP.NET app such as remote app authentication or remote app session sharing.
    /// </summary>
    public static ISystemWebAdapterRemoteClientAppBuilder AddRemoteAppClient(this ISystemWebAdapterBuilder builder, Action<RemoteAppClientOptions> configure)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        builder.Services.AddOptions<RemoteAppClientOptions>()
            .Configure(configure)
            .ValidateDataAnnotations();

        builder.Services.AddHttpClient(RemoteConstants.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<RemoteAppClientOptions>>().Value;

                if (options.BackchannelHandler is { } handler)
                {
                    return handler;
                }

                // Disable cookies in the HTTP client because the service will manage the cookie header directly
                return new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false };
            })
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<RemoteAppClientOptions>>().Value;

                client.BaseAddress = options.RemoteAppUrl;
                client.DefaultRequestHeaders.Add(options.ApiKeyHeader, options.ApiKey);
            });

        return new Builder(builder.Services);
    }

    private class Builder : ISystemWebAdapterRemoteClientAppBuilder
    {
        public Builder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
