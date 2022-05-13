using System;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// Creates IDataProtectors that can be used to store keys for
/// protecting and unprotecting auth cookies in Azure blob storage.
/// </summary>
public sealed class AzureBlobDataProtectorFactory : ICookieDataProtectorFactory
{
    private readonly BlobClient _blobClient;

    public AzureBlobDataProtectorFactory(Uri blobClientSasUri) : this(new BlobClient(blobClientSasUri))
    {
    }

    public AzureBlobDataProtectorFactory(BlobClient blobClient)
    {
        _blobClient = blobClient;
    }

    public IDataProtectionProvider CreateDataProtectionProvider(SharedAuthCookieOptions options)
    {
        var services = new ServiceCollection();
        services.AddDataProtection()
            .SetApplicationName(options.ApplicationName)
            .PersistKeysToAzureBlobStorage(_blobClient);

        using var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IDataProtectionProvider>();
    }
}
