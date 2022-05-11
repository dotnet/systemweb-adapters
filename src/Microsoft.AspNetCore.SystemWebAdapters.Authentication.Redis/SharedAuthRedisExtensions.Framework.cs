using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Interop;
using Owin;
using StackExchange.Redis;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public static class SharedAuthRedisExtensions
{
    /// <summary>
    /// Enables cookie authentication with the possibility of sharing cookies with other apps
    /// using a Redis cache to store keys for protecting and unprotecting cookies.
    /// </summary>
    /// <param name="app">The app builder to enable cookie authentication on.</param>
    /// <param name="options">Cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie settings that must be shared between apps that will share auth cookies.</param>
    /// <param name="redisUri">Connection URI for the Redis cache to use for sharing data protection keys.</param>
    /// <returns>The app builder updated with cookie authentication registered.</returns>
    public static IAppBuilder UseSharedCookieAuthenticationWithRedis(this IAppBuilder app, CookieAuthenticationOptions options, SharedAuthCookieOptions sharedOptions, string redisUri) =>
        app.UseSharedCookieAuthenticationWithRedis(options, sharedOptions, ConnectionMultiplexer.Connect(redisUri));

    /// <summary>
    /// Enables cookie authentication with the possibility of sharing cookies with other apps
    /// using a Redis cache to store keys for protecting and unprotecting cookies.
    /// </summary>
    /// <param name="app">The app builder to enable cookie authentication on.</param>
    /// <param name="options">Cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie settings that must be shared between apps that will share auth cookies.</param>
    /// <param name="redisConnectionMux">Connection multiplexer for the Redis cache to use for sharing data protection keys.</param>
    /// <returns>The app builder updated with cookie authentication registered.</returns>
    public static IAppBuilder UseSharedCookieAuthenticationWithRedis(this IAppBuilder app, CookieAuthenticationOptions options, SharedAuthCookieOptions sharedOptions, ConnectionMultiplexer redisConnectionMux)
    {
        var services = new ServiceCollection();
        services.AddDataProtection()
            .SetApplicationName(sharedOptions.ApplicationName)
            .PersistKeysToStackExchangeRedis(redisConnectionMux);

        using var serviceProvider = services.BuildServiceProvider();
        var dataProtectorProvider = serviceProvider.GetRequiredService<IDataProtectionProvider>();
        var dataProtector = dataProtectorProvider.CreateProtector(
            "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
            options.AuthenticationType,
            "v2");

        return app.UseSharedCookieAuthentication(options, sharedOptions, new DataProtectorShim(dataProtector));
    }
}
