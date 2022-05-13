using System.IO;
using Microsoft.AspNetCore.DataProtection;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// Creates IDataProtectors that can be used to store keys for
/// protecting and unprotecting auth cookies in a shared directory.
/// </summary>
public class SharedDirectoryDataProtectorFactory : ICookieDataProtectorFactory
{
    private readonly DirectoryInfo _directory;

    public SharedDirectoryDataProtectorFactory(string directoryPath) : this(new DirectoryInfo(directoryPath))
    {
    }

    public SharedDirectoryDataProtectorFactory(DirectoryInfo directory)
    {
        if (!directory.Exists)
        {
            directory.Create();
        }

        _directory = directory;
    }

    public IDataProtectionProvider CreateDataProtectionProvider(SharedAuthCookieOptions options) =>
        DataProtectionProvider.Create(_directory, builder =>
        {
            builder.SetApplicationName(options.ApplicationName);
        });
}
