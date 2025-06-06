using System.Diagnostics;
using Microsoft.Playwright.Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public abstract class DebugPageTest : PageTest
{
    public override Task InitializeAsync()
    {
        if (Debugger.IsAttached)
        {
            Environment.SetEnvironmentVariable("PWDEBUG", "1");
        }

        return base.InitializeAsync();
    }
}
