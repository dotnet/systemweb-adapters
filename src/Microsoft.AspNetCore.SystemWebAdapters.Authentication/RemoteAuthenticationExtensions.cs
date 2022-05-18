using System;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public static class RemoteAuthenticationExtensions
{
    public static AuthenticationBuilder AddRemote(this AuthenticationBuilder authenticationBuilder)
        => AddRemote(authenticationBuilder, RemoteAuthenticationDefaults.AuthenticationScheme, null);

    public static AuthenticationBuilder AddRemote(this AuthenticationBuilder authenticationBuilder, string scheme)
        => AddRemote(authenticationBuilder, scheme, null);

    public static AuthenticationBuilder AddRemote(this AuthenticationBuilder authenticationBuilder, Action<RemoteAuthenticationOptions>? configureOptions)
        => AddRemote(authenticationBuilder, RemoteAuthenticationDefaults.AuthenticationScheme, configureOptions);

    public static AuthenticationBuilder AddRemote(this AuthenticationBuilder authenticationBuilder, string scheme, Action<RemoteAuthenticationOptions>? configureOptions)
    {
        authenticationBuilder.Services.AddScoped<IRemoteAuthenticateResultProcessor, RedirectUrlProcessor>();
        authenticationBuilder.Services.AddSingleton<IAuthenticationResultFactory<RemoteAuthenticationResult>, RemoteAuthenticationResultFactory>();
        authenticationBuilder.Services.AddHttpClient<IAuthenticationService<RemoteAuthenticationResult>, RemoteAuthenticationService>()
            // Disable cookies in the HTTP client because the service will manage the cookie header directly
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false });

        authenticationBuilder.Services.AddOptions<RemoteAuthenticationOptions>(scheme)
            .Configure(configureOptions)
            .ValidateDataAnnotations();
        return authenticationBuilder.AddScheme<RemoteAuthenticationOptions, RemoteAuthenticationAuthHandler>(scheme, configureOptions);
    }

    public static ISystemWebAdapterBuilder AddRemoteAuthentication(this ISystemWebAdapterBuilder systemWebAdapterBuilder, Action<RemoteAuthenticationOptions> configureOptions)
    {
        systemWebAdapterBuilder.Services.AddAuthentication(RemoteAuthenticationDefaults.AuthenticationScheme)
            .AddRemote(configureOptions);

        return systemWebAdapterBuilder;
    }
}
