using System;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Interop;
using Owin;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public static class SharedAuthBlobExtensions
{
    /// <summary>
    /// Enables cookie authentication with the possibility of sharing cookies with other apps
    /// using a Azure Blob Storage to store keys for protecting and unprotecting cookies.
    /// </summary>
    /// <param name="app">The app builder to enable cookie authentication on.</param>
    /// <param name="options">Cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie settings that must be shared between apps that will share auth cookies.</param>
    /// <param name="blobContainerSASUri">URI for the Blob container to use for sharing data protection keys, including SAS token.</param>
    /// <returns>The app builder updated with cookie authentication registered.</returns>
    public static IAppBuilder UseSharedCookieAuthenticationWithBlobStorage(this IAppBuilder app, CookieAuthenticationOptions options, SharedAuthCookieOptions sharedOptions, Uri blobContainerSASUri)
    {
        var services = new ServiceCollection();
        services.AddDataProtection()
            .SetApplicationName(sharedOptions.ApplicationName)
            .PersistKeysToAzureBlobStorage(blobContainerSASUri);

        using var serviceProvider = services.BuildServiceProvider();
        var dataProtectorProvider = serviceProvider.GetRequiredService<IDataProtectionProvider>();
        var dataProtector = dataProtectorProvider.CreateProtector(
            "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
            options.AuthenticationType,
            "v2");

        return app.UseSharedCookieAuthentication(options, sharedOptions, new DataProtectorShim(dataProtector));
    }

    /// <summary>
    /// Enables cookie authentication with the possibility of sharing cookies with other apps
    /// using a Azure Blob Storage to store keys for protecting and unprotecting cookies.
    /// </summary>
    /// <param name="app">The app builder to enable cookie authentication on.</param>
    /// <param name="options">Cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie settings that must be shared between apps that will share auth cookies.</param>
    /// <param name="blobClient">Blob client to use for sharing data protection keys.</param>
    /// <returns>The app builder updated with cookie authentication registered.</returns>
    public static IAppBuilder UseSharedCookieAuthenticationWithBlobStorage(this IAppBuilder app, CookieAuthenticationOptions options, SharedAuthCookieOptions sharedOptions, BlobClient blobClient)
    {
        var services = new ServiceCollection();
        services.AddDataProtection()
            .SetApplicationName(sharedOptions.ApplicationName)
            .PersistKeysToAzureBlobStorage(blobClient);

        using var serviceProvider = services.BuildServiceProvider();
        var dataProtectorProvider = serviceProvider.GetRequiredService<IDataProtectionProvider>();
        var dataProtector = dataProtectorProvider.CreateProtector(
            "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
            options.AuthenticationType,
            "v2");

        return app.UseSharedCookieAuthentication(options, sharedOptions, new DataProtectorShim(dataProtector));
    }
}
