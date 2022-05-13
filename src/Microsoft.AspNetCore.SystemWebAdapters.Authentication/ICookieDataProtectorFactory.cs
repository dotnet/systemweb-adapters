using Microsoft.AspNetCore.DataProtection;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public interface ICookieDataProtectorFactory
{
    IDataProtectionProvider CreateDataProtectionProvider(SharedAuthCookieOptions options);
}
