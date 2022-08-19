using Microsoft.AspNetCore.DataProtection;
using Microsoft.Owin.Security.Interop;

namespace Microsoft.Owin.Security.Cookies;

public static class CookieExtensions
{
    public static void ConfigureSharedCookie(this CookieAuthenticationOptions options,
        string sharedApplicationName,
        string authScheme,
        string sharedKeyDirectory)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var sharedDataProtectionProvider = DataProtectionProvider.Create(
            new DirectoryInfo(sharedKeyDirectory),
                builder => builder.SetApplicationName(sharedApplicationName))
                .CreateProtector(
                    "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
                    authScheme,
                    "v2");

        options.TicketDataFormat = new AspNetTicketDataFormat(new DataProtectorShim(sharedDataProtectionProvider));
    }
}
