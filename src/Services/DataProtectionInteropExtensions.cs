// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.AspNetCore.DataProtection;

public static class DataProtectionInteropExtensions
{
    public static IDataProtector GetCookieAuthenticationDataProtector(this IDataProtectionProvider dataProtection, string name)
    {
        // There is a well-known set of purposes that allow interop with ASP.NET Cookie Authentication
        // https://github.com/dotnet/aspnetcore/blob/377049b6182c029a3dc633650e360b258fa7c07a/src/Security/Authentication/Cookies/src/PostConfigureCookieAuthenticationOptions.cs#L43
        return dataProtection.CreateProtector(
            "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
            name,
            "v2");
    }
}
