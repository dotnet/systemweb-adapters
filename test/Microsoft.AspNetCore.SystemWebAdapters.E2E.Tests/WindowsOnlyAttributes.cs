// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace Xunit;

public sealed class WindowsOnlyFact : FactAttribute
{
    public WindowsOnlyFact()
    {
        Skip = WindowsSkipReasons.OSPlatformSkipReason;
    }
}

public sealed class WindowsOnlyTheory : TheoryAttribute
{
    public WindowsOnlyTheory()
    {
        Skip = WindowsSkipReasons.OSPlatformSkipReason;
    }
}

public sealed class WindowsWithLinuxContainersTheory : TheoryAttribute
{
    public WindowsWithLinuxContainersTheory()
    {
        Skip = WindowsSkipReasons.DockerRequiredSkipVersion;
    }
}

internal static partial class WindowsSkipReasons
{
    public static string? OSPlatformSkipReason
    {
        get
        {

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "This test is only supported on Windows";
            }

            return null;
        }
    }

    public static string? DockerRequiredSkipVersion
    {
        get
        {
            if (OSPlatformSkipReason is { } osReason)
            {
                return osReason;
            }

            if (!DockerStatus.IsSupported)
            {
                var status = "This test requires Docker to be configured for Linux container";

                if (DockerStatus.OS is { } os)
                {
                    status += $", but it appears to be configured for {os} containers";
                }

                return status;
            }

            return null;
        }
    }
}
