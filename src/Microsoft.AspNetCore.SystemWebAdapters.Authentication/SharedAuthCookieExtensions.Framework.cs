using Microsoft.AspNetCore.DataProtection;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Interop;
using Owin;

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
    /// <param name="dataProtectorFactory">Factory used to create the data protection provider used to protect and unprotect the cookies.</param>
    /// <returns>The app builder updated with cookie authentication registered.</returns>
    public static IAppBuilder UseSharedCookieAuthentication(this IAppBuilder app, CookieAuthenticationOptions options, SharedAuthCookieOptions sharedOptions, ICookieDataProtectorFactory dataProtectorFactory)
    {
        var dataProtectionProvider = dataProtectorFactory.CreateDataProtectionProvider(sharedOptions);
        var protector = dataProtectionProvider.CreateProtector(
            "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
            sharedOptions.AuthenticationScheme,
            "v2");

        options.CookieName = sharedOptions.CookieName;
        options.AuthenticationType = sharedOptions.AuthenticationScheme;
        options.TicketDataFormat = new AspNetTicketDataFormat(new DataProtectorShim(protector));

        return app.UseCookieAuthentication(options);
    }
}
