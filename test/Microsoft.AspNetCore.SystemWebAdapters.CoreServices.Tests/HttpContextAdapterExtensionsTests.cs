// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters
{
    public class HttpContextAdapterExtensionsTests
    {
        [Fact]
        public void NullTypesReturnNull()
        {
            Assert.Null(((HttpContextCore)null!).GetSystemWebHttpContext());
            Assert.Null(((HttpRequestCore)null!).GetSystemWebRequest());
            Assert.Null(((HttpResponseCore)null!).GetSystemWebResponse());

            Assert.Null(((HttpContextCore)null!).GetSystemWebHttpContextBase());
            Assert.Null(((HttpRequestCore)null!).GetSystemWebRequestBase());
            Assert.Null(((HttpResponseCore)null!).GetSystemWebResponseBase());

            Assert.Null(((HttpContext)null!).GetCoreHttpContext());
            Assert.Null(((HttpRequest)null!).GetCoreRequest());
            Assert.Null(((HttpResponse)null!).GetCoreResponse());
        }

        [Fact]
        public void OriginalContextIsStored()
        {
            var context = new DefaultHttpContext();
            var adapter = new HttpContext(context);

            Assert.Same(context, adapter.GetCoreHttpContext());
        }

        [Fact]
        public void AdaptersAreCached()
        {
            var context = new DefaultHttpContext();

            var contextAdapter1 = context.GetSystemWebHttpContext();
            var contextAdapter2 = context.GetSystemWebHttpContext();
            Assert.Same(contextAdapter1, contextAdapter2);

            var requestAdapter1 = context.Request.GetSystemWebRequest();
            var requestAdapter2 = context.Request.GetSystemWebRequest();
            Assert.Same(requestAdapter1, requestAdapter2);

            var responseAdapter1 = context.Response.GetSystemWebResponse();
            var responseAdapter2 = context.Response.GetSystemWebResponse();
            Assert.Same(responseAdapter1, responseAdapter2);
        }

        [Fact]
        public void AdapterHttpContextBaseIsCached()
        {
            var context = new DefaultHttpContext();
            var adapterBase1 = context.GetSystemWebHttpContextBase();
            var adapterBase2 = context.GetSystemWebHttpContextBase();

            Assert.IsType<HttpContextWrapper>(adapterBase1);
            Assert.Same(adapterBase1, adapterBase2);
        }

        [Fact]
        public void AdapterHttpResponseBaseIsCached()
        {
            var context = new DefaultHttpContext();
            var adapterBase1 = context.Response.GetSystemWebResponseBase();
            var adapterBase2 = context.Response.GetSystemWebResponseBase();

            Assert.IsType<HttpResponseWrapper>(adapterBase1);
            Assert.Same(adapterBase1, adapterBase2);
        }

        [Fact]
        public void AdapterHttpRequestBaseIsCached()
        {
            var context = new DefaultHttpContext();
            var adapterBase1 = context.Request.GetSystemWebRequestBase();
            var adapterBase2 = context.Request.GetSystemWebRequestBase();

            Assert.IsType<HttpRequestWrapper>(adapterBase1);
            Assert.Same(adapterBase1, adapterBase2);
        }
    }
}
