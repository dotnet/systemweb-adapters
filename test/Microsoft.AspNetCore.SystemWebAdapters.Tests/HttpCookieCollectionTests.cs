// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using System.Web;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class HttpCookieCollectionTests
{
    private readonly Fixture _fixture;

    public HttpCookieCollectionTests()
    {
        _fixture = new Fixture();
    }

    [InlineData(true, true, true, "public")]
    [InlineData(true, true, false, "public")]
    [InlineData(true, false, true, "public")]
    [InlineData(true, false, false, "public, no-cache=\"Set-Cookie\"")]
    [InlineData(false, true, true, null)]
    [InlineData(false, true, false, null)]
    [InlineData(false, false, true, null)]
    [InlineData(false, false, false, null)]
    [Theory]
    public async Task ShareableCookieSetsCacheControlAsync(bool isPublic, bool isShareable, bool isHttps, string expectedCacheControl)
    {
        // Arrange
        var cookieName = _fixture.Create<string>();

        var responseCookies = new Mock<IResponseCookies>();
        var responseHeaders = new HeaderDictionary();

        if (isPublic)
        {
            responseHeaders[HeaderNames.CacheControl] = CacheControlHeaderValue.PublicString;
        }

        var requestCore = new Mock<HttpRequestCore>();
        requestCore.Setup(r => r.IsHttps).Returns(isHttps);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.Cookies).Returns(responseCookies.Object);
        responseCore.Setup(r => r.Headers).Returns(responseHeaders);

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Request).Returns(requestCore.Object);
        context.Setup(c => c.Response).Returns(responseCore.Object);
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);

        var response = new HttpResponse(responseCore.Object);
        Func<object, Task> callback = null!;

        responseCore.Setup(r => r.OnStarting(It.IsAny<Func<object, Task>>(), It.IsAny<HttpResponse>()))
            .Callback((Func<object, Task> c, object _) => callback = c);

        // Act
        response.Cookies.Add(new HttpCookie(cookieName) { Shareable = isShareable });
        await callback(response);

        // Assert
        Assert.Equal(expectedCacheControl, responseHeaders[HeaderNames.CacheControl]);
    }
}
