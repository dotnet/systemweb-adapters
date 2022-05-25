// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// Helper methods for registering remote authentication services
/// </summary>
public static class RemoteAuthenticationExtensions
{
    /// <summary>
    /// Registers remote authentication auth handler with an authentication builder using a default scheme name.
    /// </summary>
    /// <param name="authenticationBuilder">The authentication builder to register the remote authentication handler with.</param>
    /// <returns>The authentication builder updated with the remote authentication handler added.</returns>
    public static AuthenticationBuilder AddRemoteAppAuthentication(this AuthenticationBuilder authenticationBuilder)
        => AddRemoteAppAuthentication(authenticationBuilder, RemoteAuthenticationDefaults.AuthenticationScheme, null);

    /// <summary>
    /// Registers remote authentication auth handler with an authentication builder.
    /// </summary>
    /// <param name="authenticationBuilder">The authentication builder to register the remote authentication handler with.</param>
    /// <param name="scheme">The scheme name for the remote authentication handler.</param>
    /// <returns>The authentication builder updated with the remote authentication handler added using the given scheme.</returns>
    public static AuthenticationBuilder AddRemoteAppAuthentication(this AuthenticationBuilder authenticationBuilder, string scheme)
        => AddRemoteAppAuthentication(authenticationBuilder, scheme, null);

    /// <summary>
    /// Registers remote authentication auth handler with an authentication builder using a default scheme name.
    /// </summary>
    /// <param name="authenticationBuilder">The authentication builder to register the remote authentication handler with.</param>
    /// <param name="configureOptions">Configuration options for the remote authentication handler.</param>
    /// <returns>The authentication builder updated with the remote authentication handler added using the given configuration.</returns>
    public static AuthenticationBuilder AddRemoteAppAuthentication(this AuthenticationBuilder authenticationBuilder, Action<RemoteAppAuthenticationOptions>? configureOptions)
        => AddRemoteAppAuthentication(authenticationBuilder, RemoteAuthenticationDefaults.AuthenticationScheme, configureOptions);

    /// <summary>
    /// Registers remote authentication auth handler with an authentication builder.
    /// </summary>
    /// <param name="authenticationBuilder">The authentication builder to register the remote authentication handler with.</param>
    /// <param name="scheme">The scheme name for the remote authentication handler.</param>
    /// <param name="configureOptions">Configuration options for the remote authentication handler.</param>
    /// <returns>The authentication builder updated with the remote authentication handler added using the given scheme and configuration.</returns>
    public static AuthenticationBuilder AddRemoteAppAuthentication(this AuthenticationBuilder authenticationBuilder, string scheme, Action<RemoteAppAuthenticationOptions>? configureOptions)
    {
        authenticationBuilder.Services.AddScoped<IRemoteAuthenticationResultProcessor, RedirectUrlProcessor>();
        authenticationBuilder.Services.AddSingleton<IAuthenticationResultFactory, RemoteAuthenticationResultFactory>();
        authenticationBuilder.Services.AddHttpClient<IRemoteAuthenticationService, RemoteAuthenticationService>()
            // Disable cookies in the HTTP client because the service will manage the cookie header directly
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false });

        authenticationBuilder.Services.AddOptions<RemoteAppAuthenticationOptions>(scheme)
            .Configure(configureOptions)
            .ValidateDataAnnotations();
        return authenticationBuilder.AddScheme<RemoteAppAuthenticationOptions, RemoteAuthenticationAuthHandler>(scheme, configureOptions);
    }

    /// <summary>
    /// Adds remote authentication services to System.Web adapters builder.
    /// </summary>
    public static ISystemWebAdapterBuilder AddRemoteAppAuthentication(this ISystemWebAdapterBuilder systemWebAdapterBuilder, Action<RemoteAppAuthenticationOptions> configureOptions)
    {
        systemWebAdapterBuilder.Services.AddAuthentication(RemoteAuthenticationDefaults.AuthenticationScheme)
            .AddRemoteAppAuthentication(configureOptions);

        return systemWebAdapterBuilder;
    }

    /// <summary>
    /// Adds remote authentication support for System.Web adapters for the endpoint(s)
    /// </summary>
    public static TBuilder RequireRemoteAppAuthentication<TBuilder>(this TBuilder builder, IRemoteAuthenticationMetadata? metadata = null)
        where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(metadata ?? new RemoteAuthenticationAttribute());
}
