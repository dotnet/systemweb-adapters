// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;
using Xunit;
using VerifyCS = Microsoft.AspNetCore.SystemWebAdapters.Analyzers.Test.CSharpCodeFixVerifier<
    Microsoft.AspNetCore.SystemWebAdapters.Analyzers.NameValueCollectionAnalyzer,
    Microsoft.AspNetCore.SystemWebAdapters.Analyzers.NameValueCollectionCodeFixProvider>;

namespace Microsoft.AspNetCore.SystemWebAdapters.Analyzers.Test;

public class NameValueCollectionAnalyzerTests
{
    [Fact]
    public async Task Empty()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Theory]
    [InlineData(nameof(HttpRequest.Form))]
    [InlineData(nameof(HttpRequest.Headers))]
    [InlineData(nameof(HttpRequest.QueryString))]
    [InlineData(nameof(HttpRequest.ServerVariables))]
    [InlineData(nameof(HttpRequest.Params))]
    public async Task GetEnumerator(string requestMethod)
    {
        var test = $@"
    using System.Web;
    
    namespace ConsoleApplication1
    {{
        class Test
        {{
            public void Method(HttpRequest request)
            {{
                var _ = {{|#0:request.{requestMethod}.GetEnumerator()|}};
            }}
        }}
    }}";

        var expected = VerifyCS.Diagnostic("SYSWEB001").WithLocation(0).WithArguments(requestMethod, nameof(NameValueCollection.GetEnumerator));
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Theory]
    [InlineData(nameof(HttpRequest.Form))]
    [InlineData(nameof(HttpRequest.Headers))]
    [InlineData(nameof(HttpRequest.QueryString))]
    [InlineData(nameof(HttpRequest.ServerVariables))]
    [InlineData(nameof(HttpRequest.Params))]
    public async Task Get(string requestMethod)
    {
        var test = $@"
    using System.Web;
    
    namespace ConsoleApplication1
    {{
        class Test
        {{
            public void Method(HttpRequest request)
            {{
                var _ = {{|#0:request.{requestMethod}.Get(5)|}};
            }}
        }}
    }}";

        var expected = VerifyCS.Diagnostic("SYSWEB001").WithLocation(0).WithArguments(requestMethod, nameof(NameValueCollection.Get));
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Theory]
    [InlineData(nameof(HttpRequest.Form))]
    [InlineData(nameof(HttpRequest.Headers))]
    [InlineData(nameof(HttpRequest.QueryString))]
    [InlineData(nameof(HttpRequest.ServerVariables))]
    [InlineData(nameof(HttpRequest.Params))]
    public async Task GetKey(string requestMethod)
    {
        var test = $@"
    using System.Web;
    
    namespace ConsoleApplication1
    {{
        class Test
        {{
            public void Method(HttpRequest request)
            {{
                var _ = {{|#0:request.{requestMethod}.GetKey(5)|}};
            }}
        }}
    }}";

        var expected = VerifyCS.Diagnostic("SYSWEB001").WithLocation(0).WithArguments(requestMethod, nameof(NameValueCollection.GetKey));
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Theory]
    [InlineData(nameof(HttpRequest.Form))]
    [InlineData(nameof(HttpRequest.Headers))]
    [InlineData(nameof(HttpRequest.QueryString))]
    [InlineData(nameof(HttpRequest.ServerVariables))]
    [InlineData(nameof(HttpRequest.Params))]
    public async Task GetValues(string requestMethod)
    {
        var test = $@"
    using System.Web;
    
    namespace ConsoleApplication1
    {{
        class Test
        {{
            public void Method(HttpRequest request)
            {{
                var _ = {{|#0:request.{requestMethod}.GetValues(5)|}};
            }}
        }}
    }}";

        var expected = VerifyCS.Diagnostic("SYSWEB001").WithLocation(0).WithArguments(requestMethod, nameof(NameValueCollection.GetValues));
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Theory]
    [InlineData(nameof(HttpRequest.Form))]
    [InlineData(nameof(HttpRequest.Headers))]
    [InlineData(nameof(HttpRequest.QueryString))]
    [InlineData(nameof(HttpRequest.ServerVariables))]
    [InlineData(nameof(HttpRequest.Params))]
    public async Task Keys(string requestMethod)
    {
        var test = $@"
    using System.Web;
    
    namespace ConsoleApplication1
    {{
        class Test
        {{
            public void Method(HttpRequest request)
            {{
                var _ = {{|#0:request.{requestMethod}.Keys|}};
            }}
        }}
    }}";

        var expected = VerifyCS.Diagnostic("SYSWEB001").WithLocation(0).WithArguments(requestMethod, nameof(NameValueCollection.Keys));
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Theory]
    [InlineData(nameof(HttpRequest.Form))]
    [InlineData(nameof(HttpRequest.Headers))]
    [InlineData(nameof(HttpRequest.QueryString))]
    [InlineData(nameof(HttpRequest.ServerVariables))]
    [InlineData(nameof(HttpRequest.Params))]
    public async Task ThisWithInt(string requestMethod)
    {
        var test = $@"
    using System.Web;
    
    namespace ConsoleApplication1
    {{
        class Test
        {{
            public void Method(HttpRequest request)
            {{
                var _ = {{|#0:request.{requestMethod}[5]|}};
            }}
        }}
    }}";

        var expected = VerifyCS.Diagnostic("SYSWEB001").WithLocation(0).WithArguments(requestMethod, "this[]");
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Theory]
    [InlineData(nameof(HttpRequest.Form), true)]
    [InlineData(nameof(HttpRequest.Headers), true)]
    [InlineData(nameof(HttpRequest.QueryString), true)]
    [InlineData(nameof(HttpRequest.ServerVariables), false)]
    [InlineData(nameof(HttpRequest.Params), false)]
    public async Task AllKeys(string requestMethod, bool isSupported)
    {
        var test = $@"
    using System.Web;
    
    namespace ConsoleApplication1
    {{
        class Test
        {{
            public void Method(HttpRequest request)
            {{
                var _ = {{|#0:request.{requestMethod}.AllKeys|}};
            }}
        }}
    }}";

        if (isSupported)
        {
            await VerifyCS.VerifyAnalyzerAsync(test);
        }
        else
        {
            var expected = VerifyCS.Diagnostic("SYSWEB001").WithLocation(0).WithArguments(requestMethod, nameof(NameValueCollection.AllKeys));
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
    }

    [Theory]
    [InlineData(nameof(HttpRequest.Form), true)]
    [InlineData(nameof(HttpRequest.Headers), true)]
    [InlineData(nameof(HttpRequest.QueryString), true)]
    [InlineData(nameof(HttpRequest.ServerVariables), false)]
    [InlineData(nameof(HttpRequest.Params), false)]
    public async Task Count(string requestMethod, bool isSupported)
    {
        var test = $@"
    using System.Web;
    
    namespace ConsoleApplication1
    {{
        class Test
        {{
            public void Method(HttpRequest request)
            {{
                var _ = {{|#0:request.{requestMethod}.Count|}};
            }}
        }}
    }}";

        if (isSupported)
        {
            await VerifyCS.VerifyAnalyzerAsync(test);
        }
        else
        {
            var expected = VerifyCS.Diagnostic("SYSWEB001").WithLocation(0).WithArguments(requestMethod, nameof(NameValueCollection.Count));
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
    }

    [Theory]
    [InlineData(nameof(HttpRequest.Form))]
    [InlineData(nameof(HttpRequest.Headers))]
    [InlineData(nameof(HttpRequest.QueryString))]
    public async Task ReplaceKeysWithAllKeys(string requestMethod)
    {
        var test = $@"
    using System.Web;
    
    namespace ConsoleApplication1
    {{
        class Test
        {{
            public void Method(HttpRequest request)
            {{
                var _ = {{|#0:request.{requestMethod}.Keys|}};
            }}
        }}
    }}";
        var fix = $@"
    using System.Web;
    
    namespace ConsoleApplication1
    {{
        class Test
        {{
            public void Method(HttpRequest request)
            {{
                var _ = {{|#0:request.{requestMethod}.AllKeys|}};
            }}
        }}
    }}";

        await VerifyCS.VerifyCodeFixAsync(test, fix);
    }
}
