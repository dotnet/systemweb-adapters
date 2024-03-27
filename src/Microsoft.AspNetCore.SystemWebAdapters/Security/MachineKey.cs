// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.DataProtection;

namespace System.Web.Security;

public static class MachineKey
{
    /// <summary>
    /// ASP.NET Framework would use this as the primary purpose by default
    /// </summary>
    private const string MachineKeyPurpose = "User.MachineKey.Protect";

    private static IDataProtector GetProtector(string[] purposes)
    {
        ArgumentNullException.ThrowIfNull(purposes);

        VerifyPurposes(purposes);

        return HttpRuntime.WebObjectActivator.GetDataProtector(MachineKeyPurpose, purposes);
    }

    public static byte[] Protect(byte[] userData, params string[] purposes)
    {
        ArgumentNullException.ThrowIfNull(userData);

        return GetProtector(purposes).Protect(userData);
    }

    public static byte[] Unprotect(byte[] protectedData, params string[] purposes)
    {
        ArgumentNullException.ThrowIfNull(protectedData);

        return GetProtector(purposes).Unprotect(protectedData);
    }

    // .NET Framework enforced this so we'll do the same so behavior is as expected
    private static void VerifyPurposes(string[] purposes)
    {
        foreach (var purpose in purposes)
        {
            if (string.IsNullOrEmpty(purpose))
            {
                throw new ArgumentException("Purposes are not allowed to be null or whitespace-only");
            }
        }
    }
}
