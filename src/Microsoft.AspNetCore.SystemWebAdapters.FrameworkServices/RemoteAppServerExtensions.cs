// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web;

public static class RemoteAppServerExtensions
{
    /// <summary>
    /// Add configuration for connecting to a remote app for System.Web extensions that require a remote
    /// ASP.NET app such as remote app authentication or remote app session sharing.
    /// </summary>
    public static ISystemWebAdapterBuilder AddRemoteAppServer(this ISystemWebAdapterBuilder builder, Action<ISystemWebAdapterRemoteServerAppBuilder> configure)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var options = builder.Services.AddOptions<RemoteAppOptions>()
            .Validate(options => !string.IsNullOrEmpty(options.ApiKey), "ApiKey must be set")
            .Validate(options => !string.IsNullOrEmpty(options.ApiKeyHeader), "ApiKeyHeader must be set");

        configure(new Builder(builder.Services));

        return builder;
    }

    public static ISystemWebAdapterRemoteServerAppBuilder Configure(this ISystemWebAdapterRemoteServerAppBuilder builder, Action<RemoteAppOptions> configure)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        builder.Services.AddOptions<RemoteAppOptions>()
            .Configure(configure);

        return builder;
    }

    private class Builder : ISystemWebAdapterRemoteServerAppBuilder
    {
        public Builder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
