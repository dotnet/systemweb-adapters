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

        builder.Services.AddSingleton<IPostConfigureOptions<RemoteAppClientOptions>, RemoteAppClientPostConfigureOptions>();
        builder.Services.AddOptions<RemoteAppClientOptions>()
            .Configure(configure)
            .ValidateDataAnnotations();

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
