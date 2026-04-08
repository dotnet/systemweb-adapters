using System.Runtime.InteropServices;

namespace Xunit;

public sealed class WindowsOnlyWindowsOnlyFact : WindowsOnlyFactAttribute
{
    public WindowsOnlyWindowsOnlyFact()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Skip = "This test is only run on Windows";
        }
    }
}

public sealed class WindowsOnlyWindowsOnlyTheory : WindowsOnlyTheoryAttribute
{
    public WindowsOnlyWindowsOnlyTheory()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Skip = "This test is only run on Windows";
        }
    }
}
