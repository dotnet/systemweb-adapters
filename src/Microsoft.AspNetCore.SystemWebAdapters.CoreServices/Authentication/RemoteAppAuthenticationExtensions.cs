// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication.ResultProcessors;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Helper methods for registering remote authentication services
/// </summary>
public static class RemoteAppAuthenticationExtensions
{
    /// <summary>
    /// Registers remote authentication auth handler with an authentication builder using a default scheme name.
    /// </summary>
    /// <param name="authenticationBuilder">The authentication builder to register the remote authentication handler with.</param>
    /// <returns>The authentication builder updated with the remote authentication handler added.</returns>
    public static AuthenticationBuilder AddRemoteAppAuthentication(this AuthenticationBuilder authenticationBuilder)
        => AddRemoteAppAuthentication(authenticationBuilder, RemoteAppAuthenticationDefaults.AuthenticationScheme, null);

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
    public static AuthenticationBuilder AddRemoteClientAuthentication(this AuthenticationBuilder authenticationBuilder, Action<RemoteAppAuthenticationClientOptions>? configureOptions = null)
        => AddRemoteAppAuthentication(authenticationBuilder, RemoteAppAuthenticationDefaults.AuthenticationScheme, configureOptions);

    /// <summary>
    /// Registers remote authentication auth handler with an authentication builder.
    /// </summary>
    /// <param name="authenticationBuilder">The authentication builder to register the remote authentication handler with.</param>
    /// <param name="scheme">The scheme name for the remote authentication handler.</param>
    /// <param name="configureOptions">Configuration options for the remote authentication handler.</param>
    /// <returns>The authentication builder updated with the remote authentication handler added using the given scheme and configuration.</returns>
    public static AuthenticationBuilder AddRemoteAppAuthentication(this AuthenticationBuilder authenticationBuilder, string scheme, Action<RemoteAppAuthenticationClientOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(authenticationBuilder);

        authenticationBuilder.AddEmptyAuthenticationScheme();
        authenticationBuilder.Services.TryAddScoped<IRemoteAppAuthenticationResultProcessor, RedirectUrlProcessor>();
        authenticationBuilder.Services.TryAddSingleton<IAuthenticationResultFactory, RemoteAppAuthenticationResultFactory>();
        authenticationBuilder.Services.TryAddSingleton<IRemoteAppAuthenticationService, RemoteAppAuthenticationService>();
        authenticationBuilder.Services.AddOptions<RemoteAppAuthenticationClientOptions>(scheme)
            .Configure(configureOptions ?? (_ => { }))
            .ValidateDataAnnotations();
        return authenticationBuilder.AddScheme<RemoteAppAuthenticationClientOptions, RemoteAppAuthenticationAuthHandler>(scheme, configureOptions);
    }

    /// <summary>
    /// Adds remote authentication services to System.Web adapters builder.
    /// </summary>
    /// <param name="isDefaultScheme">Specifies whether the remote authentication scheme should be the default authentication scheme. If false, remote authentication will only be used for endpoints specifically requiring the remote authentication scheme.</param>
    /// <param name="configureOptions">Configuration options for the remote authentication handler.</param>
    /// <remarks>
    /// This API allows you to decide if the remote authentication is the default scheme. However, starting in .NET 7, if there is only a single scheme, it will automatically be the default. To see how to configure this, please read the documentation
    /// <see href="https://learn.microsoft.com/en-us/dotnet/core/compatibility/aspnet-core/7.0/default-authentication-scheme">here</see>.
    /// </remarks>
    public static ISystemWebAdapterRemoteClientAppBuilder AddAuthenticationClient(this ISystemWebAdapterRemoteClientAppBuilder builder, bool isDefaultScheme, Action<RemoteAppAuthenticationClientOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddAuthentication(options =>
        {
            if (isDefaultScheme)
            {
                options.DefaultScheme = RemoteAppAuthenticationDefaults.AuthenticationScheme;
            }
        }).AddRemoteClientAuthentication(configureOptions);

        return builder;
    }

    /// <summary>
    /// This adds an empty authentication scheme so that no default authentication is used.
    /// See https://github.com/dotnet/aspnetcore/issues/44661 for more details and context.
    /// </summary>
    private static void AddEmptyAuthenticationScheme(this AuthenticationBuilder authenticationBuilder)
    {
        authenticationBuilder.AddScheme<AuthenticationSchemeOptions, EmptyAuthenticationHandler>($"__SystemWebAdapters_{Guid.NewGuid()}", displayName: null, _ => { });
    }

    private sealed class EmptyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public EmptyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
    }
}
