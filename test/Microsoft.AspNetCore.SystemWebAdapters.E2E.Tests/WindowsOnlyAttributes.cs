using System.Runtime.InteropServices;
using System.Diagnostics;

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

internal static class WindowsWithLinuxContainersSupport
{
    public static string? SkipReason { get; } = GetSkipReason();

    private static string? GetSkipReason()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "This test is only run on Windows";
        }

        if (!CanRunLinuxContainers())
        {
            return "This test requires Docker to be configured for Linux containers";
        }

        return null;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    private static bool CanRunLinuxContainers()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "version --format {{.Server.Os}}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            if (process is null)
            {
                return false;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return process.ExitCode == 0
                && output.Trim().Equals("linux", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
