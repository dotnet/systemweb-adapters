// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// Helper methods for registering remote authentication endpoints.
/// </summary>
public static class RemoteAppAuthenticationExtensions
{
    /// <summary>
    /// Adds the remote authentication module to System.Web adapter configuration.
    /// </summary>
    /// <param name="builder">The System.Web adapter builder to modify.</param>
    /// <param name="configure">Configuration to use when registering the remote authentication module.</param>
    /// <returns>The System.Web adapter builder updated to include the remote authentication module.</returns>
    public static ISystemWebAdapterBuilder AddRemoteAppAuthentication(this ISystemWebAdapterBuilder builder, Action<RemoteAppAuthenticationOptions>? configure = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.AddSingleton<IHttpModule, RemoteAppAuthenticationModule>();
        var options = builder.Services.AddOptions<RemoteAppAuthenticationOptions>();

        if (configure is not null)
        {
            options.Configure(configure);
        }

        return builder;
    }
}
