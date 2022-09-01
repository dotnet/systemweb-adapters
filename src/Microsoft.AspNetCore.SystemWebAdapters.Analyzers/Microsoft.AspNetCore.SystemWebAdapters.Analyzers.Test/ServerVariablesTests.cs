using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.AspNetCore.SystemWebAdapters.Analyzers.Test.CSharpAnalyzerVerifier<
    Microsoft.AspNetCore.SystemWebAdapters.Analyzers.ServerVariableAnalyzer>;

namespace Microsoft.AspNetCore.SystemWebAdapters.Analyzers.Test;

public class MicrosoftAspNetCoreSystemWebAdaptersAnalyzersUnitTest
{
    //No diagnostics expected to show up
    [Fact]
    public async Task Empty()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    //Diagnostic and CodeFix both triggered and checked for
    [Fact]
    public async Task TestMethod2()
    {
        var test = @"
    using System.Web;
    
    namespace ConsoleApplication1
    {
        class Test
        {
            public void Method(HttpRequest request)
            {
                var _ = {|#0:request.ServerVariables[0]|};
            }
        }
    }";

        var expected = VerifyCS.Diagnostic("SYSWEB001").WithLocation(0);
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }
}
