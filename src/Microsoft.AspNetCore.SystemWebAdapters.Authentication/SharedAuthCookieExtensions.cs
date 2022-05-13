using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// Extension methods for registering shared cookie services.
public static class SharedAuthCookieExtensions
{
    /// <summary>
    /// Update an existing ASP.NET Core Identity registration to use cookies compatible with
    /// shared cookie authentication. This method assumes that it is run after IServiceCollection.AddIdentity or .AddDefaultIdentity.
    /// </summary>
    /// <param name="builder">The ISystemWebAdapterBuilder configuration to configure cookie services for.</param>
    /// <param name="configureCookieOptions">Configuration method for cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie sharing, including cookie name and applicaiton name.</param>
    /// <param name="protectorFactory">A factory object capable of producing an IDataProtector for protecting and unprotecting cookies.</param>
    /// <returns>The ISystemWebAdapterBuilder updated with cookie authentication updated to be shareable with other apps.</returns>
    public static ISystemWebAdapterBuilder ConfigureSharedIdentityAuthentication(this ISystemWebAdapterBuilder builder, Action<CookieAuthenticationOptions>? configureCookieOptions, SharedAuthCookieOptions sharedOptions, ICookieDataProtectorFactory protectorFactory)
    {
        var provider = DataProtectionProvider.Create(new DirectoryInfo(Path.Combine(Path.GetTempPath(), sharedOptions.ApplicationName)), builder =>
        {
            builder.SetApplicationName(sharedOptions.ApplicationName);
        });

        builder.Services.ConfigureApplicationCookie(options =>
        {
            configureCookieOptions?.Invoke(options);
            options.Cookie.Name = sharedOptions.CookieName;
            options.DataProtectionProvider = provider;
        });

        return builder;
    }


    /// <summary>
    /// Add cookie authentication with options set such that cookies can
    /// be shared with other apps, including ASP.NET applications using .NET Framework
    /// versions of this API.
    /// </summary>
    /// <param name="builder">The ISystemWebAdapterBuilder configuration to add cookie services to.</param>
    /// <param name="configureAuthenticationOptions">Configuration method for applicaiton authentication options.</param>
    /// <param name="configureCookieOptions">Configuration method for cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie sharing, including cookie name and applicaiton name.</param>
    /// <param name="protectorFactory">A factory object capable of producing an IDataProtector for protecting and unprotecting cookies.</param>
    /// /// <returns>The ISystemWebAdapterBuilder with cookie authentication enabled such that auth cookies are shareable with other apps.</returns>
    public static ISystemWebAdapterBuilder AddSharedCookieAuthentication(this ISystemWebAdapterBuilder builder,
                                                                         Action<AuthenticationOptions>? configureAuthenticationOptions,
                                                                         Action<CookieAuthenticationOptions>? configureCookieOptions,
                                                                         SharedAuthCookieOptions sharedOptions,
                                                                         ICookieDataProtectorFactory protectorFactory)
    {
        var provider = DataProtectionProvider.Create(new DirectoryInfo(Path.Combine(Path.GetTempPath(), sharedOptions.ApplicationName)), builder =>
        {
            builder.SetApplicationName(sharedOptions.ApplicationName);
        });

        builder.Services.AddAuthentication(configureAuthenticationOptions ?? (options => { }))
            .AddCookie(sharedOptions.AuthenticationScheme, options =>
            {
                configureCookieOptions?.Invoke(options);
                options.Cookie.Name = sharedOptions.CookieName;
                options.DataProtectionProvider = provider;
            });

        return builder;
    }
}
