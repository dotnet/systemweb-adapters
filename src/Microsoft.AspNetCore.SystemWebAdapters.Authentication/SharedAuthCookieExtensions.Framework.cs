using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Interop;
using Owin;
using IDataProtector = Microsoft.Owin.Security.DataProtection.IDataProtector;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public static class SharedAuthCookieExtensions
{
    /// <summary>
    /// Enables cookie authentication given options and data protector
    /// to allow sharing the authentication cookies between apps.
    /// </summary>
    /// <param name="app">The app builder to enable cookie authentication on.</param>
    /// <param name="options">Cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie settings that must be shared between apps that will share auth cookies.</param>
    /// <param name="dataProtector">The data protector to use to protect and unprotect the cookies.</param>
    /// <returns>The app builder updated with cookie authentication registered.</returns>
    public static IAppBuilder UseSharedCookieAuthentication(this IAppBuilder app, CookieAuthenticationOptions options, SharedAuthCookieOptions sharedOptions, IDataProtector dataProtector)
    {
        options.CookieName = sharedOptions.CookieName;
        options.TicketDataFormat = new AspNetTicketDataFormat(dataProtector);

        return app.UseCookieAuthentication(options);
    }

    /// <summary>
    /// Enables cookie authentication with the possibility of sharing cookies with other apps
    /// using a file share to store keys for protecting and unprotecting cookies.
    /// </summary>
    /// <param name="app">The app builder to enable cookie authentication on.</param>
    /// <param name="options">Cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie settings that must be shared between apps that will share auth cookies.</param>
    /// <param name="redisUri">Connection URI for the Redis cache to use for sharing data protection keys.</param>
    /// <returns>The app builder updated with cookie authentication registered.</returns>
    public static IAppBuilder UseSharedCookieAuthenticationWithSharedDirectory(this IAppBuilder app, CookieAuthenticationOptions options, SharedAuthCookieOptions sharedOptions, DirectoryInfo keyRingDir)
    {
        var dataProtectorProvider = DataProtectionProvider.Create(keyRingDir, builder =>
        {
            builder.SetApplicationName(sharedOptions.ApplicationName);
        });

        var dataProtector = dataProtectorProvider.CreateProtector(
            "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
            options.AuthenticationType,
            "v2");

        return app.UseSharedCookieAuthentication(options, sharedOptions, new DataProtectorShim(dataProtector));
    }
}
