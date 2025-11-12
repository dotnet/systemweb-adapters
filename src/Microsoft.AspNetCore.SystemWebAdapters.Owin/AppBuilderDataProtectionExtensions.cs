// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Owin.Security.Interop;
using Owin;

using AspNetCoreDataProtectionProvider = Microsoft.AspNetCore.DataProtection.IDataProtectionProvider;
using OwinProtectionProvider = Microsoft.Owin.Security.DataProtection.IDataProtectionProvider;

namespace Microsoft.Owin.Security.DataProtection;

public static class AppBuilderDataProtectionExtensions
{
    public static void SetDataProtectionProvider(this IAppBuilder app, AspNetCoreDataProtectionProvider dataProtectionProvider)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(dataProtectionProvider);

        app.SetDataProtectionProvider(new DataProtectionProviderShim(dataProtectionProvider));
    }

    private sealed class DataProtectionProviderShim(AspNetCoreDataProtectionProvider other) : OwinProtectionProvider
    {
        public IDataProtector Create(params string[] purposes) => new DataProtectorShim(other.CreateProtector(purposes));
    }
}
