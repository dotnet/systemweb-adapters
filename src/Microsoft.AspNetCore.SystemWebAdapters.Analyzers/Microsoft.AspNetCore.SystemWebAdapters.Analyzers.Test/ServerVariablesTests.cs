using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;
using Xunit;
using VerifyCS = Microsoft.AspNetCore.SystemWebAdapters.Analyzers.Test.CSharpAnalyzerVerifier<
    Microsoft.AspNetCore.SystemWebAdapters.Analyzers.NameValueCollectionAnalyzer>;

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

    [Fact]
    public async Task CallThis()
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

        var expected = VerifyCS.Diagnostic("SYSWEB001").WithLocation(0).WithArguments(nameof(HttpRequest.ServerVariables), "this[]");
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task CallGet()
    {
        var test = @"
    using System.Web;
    
    namespace ConsoleApplication1
    {
        class Test
        {
            public void Method(HttpRequest request)
            {
                var _ = {|#0:request.ServerVariables.Get(0)|};
            }
        }
    }";

        var expected = VerifyCS.Diagnostic("SYSWEB001").WithLocation(0).WithArguments(nameof(HttpRequest.ServerVariables), nameof(NameValueCollection.Get));
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ServerVariablesAllKeys()
    {
        var test = @"
    using System.Web;
    
    namespace ConsoleApplication1
    {
        class Test
        {
            public void Method(HttpRequest request)
            {
                var _ = {|#0:request.ServerVariables.AllKeys|};
            }
        }
    }";

        var expected = VerifyCS.Diagnostic("SYSWEB001").WithLocation(0).WithArguments(nameof(HttpRequest.ServerVariables), nameof(NameValueCollection.AllKeys));
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task FormGetEnumerator()
    {
        var test = @"
    using System.Web;
    
    namespace ConsoleApplication1
    {
        class Test
        {
            public void Method(HttpRequest request)
            {
                var _ = {|#0:request.Form.GetEnumerator()|};
            }
        }
    }";

        var expected = VerifyCS.Diagnostic("SYSWEB001").WithLocation(0).WithArguments(nameof(HttpRequest.Form), nameof(NameValueCollection.GetEnumerator));
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }
}
