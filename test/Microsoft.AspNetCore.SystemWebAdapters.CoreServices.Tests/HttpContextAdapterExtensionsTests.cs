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
            Assert.Null((HttpContext)(HttpContextCore)null!);
            Assert.Null((HttpRequest)(HttpRequestCore)null!);
            Assert.Null((HttpResponse)(HttpResponseCore)null!);


            Assert.Null((HttpContextCore)(HttpContext)null!);
            Assert.Null((HttpRequestCore)(HttpRequest)null!);
            Assert.Null((HttpResponseCore)(HttpResponse)null!);
        }

        [Fact]
        public void OriginalContextIsStored()
        {
            var context = new DefaultHttpContext();
            var adapter = new HttpContext(context);

            Assert.Same(context, adapter.AsAspNetCore());
        }

        [Fact]
        public void AdaptersAreCached()
        {
            var context = new DefaultHttpContext();

            var contextAdapter1 = context.AsSystemWeb();
            var contextAdapter2 = context.AsSystemWeb();
            Assert.Same(contextAdapter1, contextAdapter2);

            var requestAdapter1 = context.Request.AsSystemWeb();
            var requestAdapter2 = context.Request.AsSystemWeb();
            Assert.Same(requestAdapter1, requestAdapter2);

            var responseAdapter1 = context.Response.AsSystemWeb();
            var responseAdapter2 = context.Response.AsSystemWeb();
            Assert.Same(responseAdapter1, responseAdapter2);
        }

        [Fact]
        public void AdapterHttpContextBaseIsCached()
        {
            var context = new DefaultHttpContext();
            var adapterBase1 = context.AsSystemWebBase();
            var adapterBase2 = context.AsSystemWebBase();

            Assert.IsType<HttpContextWrapper>(adapterBase1);
            Assert.Same(adapterBase1, adapterBase2);
        }

        [Fact]
        public void AdapterHttpResponseBaseIsCached()
        {
            var context = new DefaultHttpContext();
            var adapterBase1 = (HttpResponseBase)context.Response;
            var adapterBase2 = (HttpResponseBase)context.Response;

            Assert.IsType<HttpResponseWrapper>(adapterBase1);
            Assert.Same(adapterBase1, adapterBase2);
        }

        [Fact]
        public void AdapterHttpRequestBaseIsCached()
        {
            var context = new DefaultHttpContext();
            var adapterBase1 = (HttpRequestBase)context.Request;
            var adapterBase2 = (HttpRequestBase)context.Request;

            Assert.IsType<HttpRequestWrapper>(adapterBase1);
            Assert.Same(adapterBase1, adapterBase2);
        }
    }
}
