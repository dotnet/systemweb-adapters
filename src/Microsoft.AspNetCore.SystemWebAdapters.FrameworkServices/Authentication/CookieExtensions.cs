using Microsoft.AspNetCore.DataProtection;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Interop;
using Owin;

namespace Microsoft.Owin.Security.Cookies;

public static class CookieExtensions
{
    public static CookieAuthenticationOptions ConfigureSharedCookie(this IAppBuilder app, string sharedApplicationName,
        string cookieName,
        string authScheme,
        string sharedKeyDirectory,
        CookieAuthenticationOptions options)
    {
        var sharedDataProtectionProvider = DataProtectionProvider.Create(
            new DirectoryInfo(sharedKeyDirectory),
                builder => builder.SetApplicationName(sharedApplicationName))
                .CreateProtector(
                    "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
                    authScheme,
                    "v2");

        // Settings to configure shared cookie with MvcCoreApp
        options.CookieName = cookieName;
        options.TicketDataFormat = new AspNetTicketDataFormat(new DataProtectorShim(sharedDataProtectionProvider));
        return options;
    }
}
