// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

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
    public static AuthenticationBuilder AddRemote(this AuthenticationBuilder authenticationBuilder)
        => AddRemote(authenticationBuilder, RemoteAuthenticationDefaults.AuthenticationScheme, null);

    /// <summary>
    /// Registers remote authentication auth handler with an authentication builder.
    /// </summary>
    /// <param name="authenticationBuilder">The authentication builder to register the remote authentication handler with.</param>
    /// <param name="scheme">The scheme name for the remote authentication handler.</param>
    /// <returns>The authentication builder updated with the remote authentication handler added using the given scheme.</returns>
    public static AuthenticationBuilder AddRemote(this AuthenticationBuilder authenticationBuilder, string scheme)
        => AddRemote(authenticationBuilder, scheme, null);

    /// <summary>
    /// Registers remote authentication auth handler with an authentication builder using a default scheme name.
    /// </summary>
    /// <param name="authenticationBuilder">The authentication builder to register the remote authentication handler with.</param>
    /// <param name="configureOptions">Configuration options for the remote authentication handler.</param>
    /// <returns>The authentication builder updated with the remote authentication handler added using the given configuration.</returns>
    public static AuthenticationBuilder AddRemote(this AuthenticationBuilder authenticationBuilder, Action<RemoteAuthenticationOptions>? configureOptions)
        => AddRemote(authenticationBuilder, RemoteAuthenticationDefaults.AuthenticationScheme, configureOptions);

    /// <summary>
    /// Registers remote authentication auth handler with an authentication builder.
    /// </summary>
    /// <param name="authenticationBuilder">The authentication builder to register the remote authentication handler with.</param>
    /// <param name="scheme">The scheme name for the remote authentication handler.</param>
    /// <param name="configureOptions">Configuration options for the remote authentication handler.</param>
    /// <returns>The authentication builder updated with the remote authentication handler added using the given scheme and configuration.</returns>
    public static AuthenticationBuilder AddRemote(this AuthenticationBuilder authenticationBuilder, string scheme, Action<RemoteAuthenticationOptions>? configureOptions)
    {
        authenticationBuilder.Services.AddScoped<IRemoteAuthenticationResultProcessor, RedirectUrlProcessor>();
        authenticationBuilder.Services.AddSingleton<IAuthenticationResultFactory, RemoteAuthenticationResultFactory>();
        authenticationBuilder.Services.AddHttpClient<IRemoteAuthenticationService, RemoteAuthenticationService>()
            // Disable cookies in the HTTP client because the service will manage the cookie header directly
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false });

        authenticationBuilder.Services.AddOptions<RemoteAuthenticationOptions>(scheme)
            .Configure(configureOptions)
            .ValidateDataAnnotations();
        return authenticationBuilder.AddScheme<RemoteAuthenticationOptions, RemoteAuthenticationAuthHandler>(scheme, configureOptions);
    }

    /// <summary>
    /// Adds remote authentication services to System.Web adapters builder.
    /// </summary>
    public static ISystemWebAdapterBuilder AddRemoteAuthentication(this ISystemWebAdapterBuilder systemWebAdapterBuilder, Action<RemoteAuthenticationOptions> configureOptions)
    {
        systemWebAdapterBuilder.Services.AddAuthentication(RemoteAuthenticationDefaults.AuthenticationScheme)
            .AddRemote(configureOptions);

        return systemWebAdapterBuilder;
    }

    /// <summary>
    /// Adds remote authentication support for System.Web adapters for the endpoint(s)
    /// </summary>
    public static TBuilder RequireRemoteAuthentication<TBuilder>(this TBuilder builder, IRemoteAuthenticationMetadata? metadata = null)
        where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(metadata ?? new RemoteAuthenticationAttribute());
}
