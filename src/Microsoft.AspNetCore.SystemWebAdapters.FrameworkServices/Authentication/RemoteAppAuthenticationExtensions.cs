// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web;

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
    public static ISystemWebAdapterRemoteServerAppBuilder AddAuthenticationServer(this ISystemWebAdapterRemoteServerAppBuilder builder, Action<RemoteAppAuthenticationServerOptions>? configure = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.AddScoped<IHttpModule, RemoteAppAuthenticationModule>();
        builder.Services.AddScoped<IClaimsSerializer, BinaryClaimsSerializer>();
        var options = builder.Services.AddOptions<RemoteAppAuthenticationServerOptions>()
            .ValidateDataAnnotations();

        if (configure is not null)
        {
            options.Configure(configure);
        }

        return builder;
    }
}
