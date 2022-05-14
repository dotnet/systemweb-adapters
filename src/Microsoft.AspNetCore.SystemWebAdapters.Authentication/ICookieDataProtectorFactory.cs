using Microsoft.AspNetCore.DataProtection;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// Interface that can generate data protection providers for protecting
/// and unprotecting cookies.
/// </summary>
public interface ICookieDataProtectorFactory
{
    IDataProtectionProvider CreateDataProtectionProvider(SharedAuthCookieOptions options);
}
