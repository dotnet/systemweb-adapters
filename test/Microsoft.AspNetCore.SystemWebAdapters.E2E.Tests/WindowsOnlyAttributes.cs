using System.Runtime.InteropServices;

namespace Xunit;

public sealed class WindowsOnlyFact : FactAttribute
{
    public WindowsOnlyFact()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Skip = "This test is only run on Windows";
        }
    }
}

public sealed class WindowsOnlyTheory : TheoryAttribute
{
    public WindowsOnlyTheory()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Skip = "This test is only run on Windows";
        }
    }
}

public sealed class WindowsWithLinuxContainersTheory : TheoryAttribute
{
    public WindowsWithLinuxContainersTheory()
    {
        Skip = WindowsWithLinuxContainersSupport.SkipReason;
    }
}

internal static partial class WindowsWithLinuxContainersSupport
{
    public static string? SkipReason { get; } = GetSkipReason();

    private static string? GetSkipReason()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "This test is only run on Windows";
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
