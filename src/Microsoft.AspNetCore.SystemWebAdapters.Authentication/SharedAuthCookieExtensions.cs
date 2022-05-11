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
    /// Add cookie authentication with options set such that cookies can
    /// be shared with other apps, including ASP.NET applications using .NET Framework
    /// versions of this API.
    /// </summary>
    /// <param name="authenticationBuilder">The authentication configuration to add cookie services to. This can be obtained by calling services.AddAuthentication.</param>
    /// <param name="optionsConfiguration">Configuration method for cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie sharing, including cookie name and applicaiton name.</param>
    /// <param name="configureDataProtection">Configuration method for setting up data protection system to be used for protecting and unprotecting authentication cookies.</param>
    /// <param name="authenticationScheme">The authentication scheme for authentication with the shared cookie. This must match the authentication type used in the ASP.NET app. Defaults to "Cookies".</param>
    public static void AddSharedCookieAuthentication(this AuthenticationBuilder authenticationBuilder, Action<CookieAuthenticationOptions>? optionsConfiguration, SharedAuthCookieOptions sharedOptions, Action<IDataProtectionBuilder> configureDataProtection, string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme)
    {
        // Enable cookie authentication with the specified cookie name
        authenticationBuilder
            .AddCookie(authenticationScheme, options =>
            {
                optionsConfiguration?.Invoke(options);
                options.Cookie.Name = sharedOptions.CookieName;
            });

        // Ensure that data protection uses the application name that is shared between
        // applications sharing auth cookies, but unique to other apps
        var dataProtectionBuilder = authenticationBuilder.Services.AddDataProtection()
            .SetApplicationName(sharedOptions.ApplicationName);

        // Use user-provided callback to configure data protection services
        configureDataProtection(dataProtectionBuilder);
    }

    /// <summary>
    /// Add cookie authentication services with cookie options set such that cookies can
    /// be shared with other apps, including ASP.NET applications using .NET Framework
    /// versions of this API.
    /// </summary>
    /// <param name="services">The service collection to add authentication services to.</param>
    /// <param name="optionsConfiguration">Configuration method for cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie sharing, including cookie name and applicaiton name.</param>
    /// <param name="configureDataProtection">Configuration method for setting up data protection system to be used for protecting and unprotecting authentication cookies.</param>
    /// <param name="authenticationScheme">The authentication scheme for authentication with the shared cookie. This must match the authentication type used in the ASP.NET app. Defaults to "Cookies".</param>
    public static void AddSharedCookieAuthentication(this IServiceCollection services, Action<CookieAuthenticationOptions>? optionsConfiguration, SharedAuthCookieOptions sharedOptions, Action<IDataProtectionBuilder> configureDataProtection, string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme) =>
        AddSharedCookieAuthentication(services.AddAuthentication(authenticationScheme), optionsConfiguration, sharedOptions, configureDataProtection, authenticationScheme);

    /// <summary>
    /// Add cookie authentication with cookie options set such that cookies can
    /// be shared with other apps, using a shared folder to share keys for data protection.
    /// </summary>
    /// <param name="authenticationBuilder">The authentication configuration to add cookie services to. This can be obtained by calling services.AddAuthentication.</param>
    /// <param name="optionsConfiguration">Configuration method for cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie sharing, including cookie name and applicaiton name.</param>
    /// <param name="keyRingDir">The directory to use for sharing data protection keys.</param>
    /// <param name="authenticationScheme">The authentication scheme for authentication with the shared cookie. This must match the authentication type used in the ASP.NET app. Defaults to "Cookies".</param>
    public static void AddSharedCookieAuthenticationWithSharedDirectory(this AuthenticationBuilder authenticationBuilder, Action<CookieAuthenticationOptions>? optionsConfiguration, SharedAuthCookieOptions sharedOptions, DirectoryInfo keyRingDir, string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme) =>
        authenticationBuilder.AddSharedCookieAuthentication(optionsConfiguration, sharedOptions, dpb => dpb.PersistKeysToFileSystem(keyRingDir), authenticationScheme);

    /// <summary>
    /// Add cookie authentication services with cookie options set such that cookies can
    /// be shared with other apps, using a shared folder to share keys for data protection.
    /// </summary>
    /// <param name="services">The service collection to add authentication services to.</param>
    /// <param name="optionsConfiguration">Configuration method for cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie sharing, including cookie name and applicaiton name.</param>
    /// <param name="keyRingDir">The directory to use for sharing data protection keys.</param>
    /// <param name="authenticationScheme">The authentication scheme for authentication with the shared cookie. This must match the authentication type used in the ASP.NET app. Defaults to "Cookies".</param>
    public static void AddSharedCookieAuthenticationWithSharedDirectory(this ServiceCollection services, Action<CookieAuthenticationOptions>? optionsConfiguration, SharedAuthCookieOptions sharedOptions, DirectoryInfo keyRingDir, string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme) =>
        services.AddSharedCookieAuthentication(optionsConfiguration, sharedOptions, dpb => dpb.PersistKeysToFileSystem(keyRingDir), authenticationScheme);
}
