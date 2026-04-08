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
