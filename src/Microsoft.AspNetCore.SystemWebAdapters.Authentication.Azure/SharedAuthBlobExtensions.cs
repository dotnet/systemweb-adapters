using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// Extension methods for registering shared cookie services using Azure Blob Storage to share data protection keys.
/// </summary>
public static class SharedAuthBlobExtensions
{
    /// <summary>
    /// Add cookie authentication with cookie options set such that cookies can
    /// be shared with other apps, using Azure Blob Storage to share keys for data protection.
    /// </summary>
    /// <param name="authenticationBuilder">The authentication configuration to add cookie services to. This can be obtained by calling services.AddAuthentication.</param>
    /// <param name="optionsConfiguration">Configuration method for cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie sharing, including cookie name and applicaiton name.</param>
    /// <param name="blobContainerSASUri">URI for the Blob container to use for sharing data protection keys, including SAS token.</param>
    /// <param name="authenticationScheme">The authentication scheme for authentication with the shared cookie. This must match the authentication type used in the ASP.NET app. Defaults to "Cookies".</param>
    public static void AddSharedCookieAuthenticationWithBlobStorage(this AuthenticationBuilder authenticationBuilder, Action<CookieAuthenticationOptions>? optionsConfiguration, SharedAuthCookieOptions sharedOptions, string blobContainerSASUri, string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme) =>
        authenticationBuilder.AddSharedCookieAuthentication(optionsConfiguration, sharedOptions, dpb => dpb.PersistKeysToAzureBlobStorage(new Uri(blobContainerSASUri)), authenticationScheme);

    /// <summary>
    /// Add cookie authentication with cookie options set such that cookies can
    /// be shared with other apps, using Azure Blob Storage to share keys for data protection.
    /// </summary>
    /// <param name="authenticationBuilder">The authentication configuration to add cookie services to. This can be obtained by calling services.AddAuthentication.</param>
    /// <param name="optionsConfiguration">Configuration method for cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie sharing, including cookie name and applicaiton name.</param>
    /// <param name="blobClient">Blob client to use for sharing data protection keys.</param>
    /// <param name="authenticationScheme">The authentication scheme for authentication with the shared cookie. This must match the authentication type used in the ASP.NET app. Defaults to "Cookies".</param>
    public static void AddSharedCookieAuthenticationWithBlobStorage(this AuthenticationBuilder authenticationBuilder, Action<CookieAuthenticationOptions>? optionsConfiguration, SharedAuthCookieOptions sharedOptions, BlobClient blobClient, string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme) =>
        authenticationBuilder.AddSharedCookieAuthentication(optionsConfiguration, sharedOptions, dpb => dpb.PersistKeysToAzureBlobStorage(blobClient), authenticationScheme);

    /// <summary>
    /// Add cookie authentication services with cookie options set such that cookies can
    /// be shared with other apps, using Azure Blob Storage to share keys for data protection.
    /// </summary>
    /// <param name="services">The service collection to add authentication services to.</param>
    /// <param name="optionsConfiguration">Configuration method for cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie sharing, including cookie name and applicaiton name.</param>
    /// <param name="blobContainerSASUri">URI for the Blob container to use for sharing data protection keys, including SAS token.</param>
    /// <param name="authenticationScheme">The authentication scheme for authentication with the shared cookie. This must match the authentication type used in the ASP.NET app. Defaults to "Cookies".</param>
    public static void AddSharedCookieAuthenticationWithBlobStorage(this ServiceCollection services, Action<CookieAuthenticationOptions>? optionsConfiguration, SharedAuthCookieOptions sharedOptions, string blobContainerSASUri, string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme) =>
        services.AddSharedCookieAuthentication(optionsConfiguration, sharedOptions, dpb => dpb.PersistKeysToAzureBlobStorage(new Uri(blobContainerSASUri)), authenticationScheme);

    /// <summary>
    /// Add cookie authentication services with cookie options set such that cookies can
    /// be shared with other apps, using Azure Blob Storage to share keys for data protection.
    /// </summary>
    /// <param name="services">The service collection to add authentication services to.</param>
    /// <param name="optionsConfiguration">Configuration method for cookie authentication options.</param>
    /// <param name="sharedOptions">Options for cookie sharing, including cookie name and applicaiton name.</param>
    /// <param name="blobClient">Blob client to use for sharing data protection keys.</param>
    /// <param name="authenticationScheme">The authentication scheme for authentication with the shared cookie. This must match the authentication type used in the ASP.NET app. Defaults to "Cookies".</param>
    public static void AddSharedCookieAuthenticationWithBlobStorage(this ServiceCollection services, Action<CookieAuthenticationOptions>? optionsConfiguration, SharedAuthCookieOptions sharedOptions, BlobClient blobClient, string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme) =>
        services.AddSharedCookieAuthentication(optionsConfiguration, sharedOptions, dpb => dpb.PersistKeysToAzureBlobStorage(blobClient), authenticationScheme);
}
