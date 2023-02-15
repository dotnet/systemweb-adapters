// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Internal;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class HttpResponseTests
{
    private readonly Fixture _fixture;

    public HttpResponseTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void StatusCode()
    {
        // Arrange
        var responseCore = new Mock<HttpResponseCore>();
        responseCore.SetupProperty(r => r.StatusCode);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.StatusCode = 205;

        // Assert
        Assert.Equal(205, response.StatusCode);
        Assert.Equal(205, responseCore.Object.StatusCode);
    }

    [Fact]
    public void StatusDescription()
    {
        // Arrange
        var description = _fixture.Create<string>();
        var feature = new Mock<IHttpResponseFeature>();
        feature.SetupProperty(f => f.ReasonPhrase);

        var features = new Mock<IFeatureCollection>();
        features.Setup(f => f.Get<IHttpResponseFeature>()).Returns(feature.Object);

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features.Object);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);
        responseCore.SetupProperty(r => r.StatusCode);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.StatusDescription = description;

        // Assert
        Assert.Equal(description, feature.Object.ReasonPhrase);
        Assert.Equal(description, response.StatusDescription);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TrySkipIisCustomErrors(bool isEnabled)
    {
        // Arrange
        var description = _fixture.Create<string>();
        var feature = new Mock<IStatusCodePagesFeature>();
        feature.SetupProperty(f => f.Enabled);

        var features = new Mock<IFeatureCollection>();
        features.Setup(f => f.Get<IStatusCodePagesFeature>()).Returns(feature.Object);

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features.Object);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);
        responseCore.SetupProperty(r => r.StatusCode);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.TrySkipIisCustomErrors = isEnabled;

        // Assert
        Assert.Equal(isEnabled, feature.Object.Enabled);
        Assert.Equal(isEnabled, response.TrySkipIisCustomErrors);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SuppressContent(bool suppressContent)
    {
        // Arrange
        var feature = new Mock<IHttpResponseBufferingFeature>();
        feature.SetupProperty(f => f.SuppressContent);

        var features = new Mock<IFeatureCollection>();
        features.Setup(f => f.Get<IHttpResponseBufferingFeature>()).Returns(feature.Object);

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features.Object);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);
        responseCore.SetupProperty(r => r.StatusCode);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.SuppressContent = suppressContent;

        // Assert
        Assert.Equal(suppressContent, feature.Object.SuppressContent);
        Assert.Equal(suppressContent, response.SuppressContent);
    }

    [Fact]
    public void End()
    {
        // Arrange
        var feature = new Mock<IHttpResponseEndFeature>();

        var features = new FeatureCollection();
        features.Set(feature.Object);

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);
        responseCore.SetupProperty(r => r.StatusCode);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.End();

        // Assert
        feature.Verify(f => f.EndAsync(), Times.Once);
    }

    [Fact]
    public void EndNoFeature()
    {
        // Arrange
        var features = new Mock<IFeatureCollection>();

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features.Object);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);
        responseCore.SetupProperty(r => r.StatusCode);

        var response = new HttpResponse(responseCore.Object);

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => response.End());
    }

    [Fact]
    public void Flush()
    {
        // Arrange
        var responseCore = new Mock<HttpResponseCore>();
        var stream = new Mock<Stream>();
        responseCore.Setup(r => r.Body).Returns(stream.Object);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.Flush();

        // Assert
        stream.Verify(s => s.Flush(), Times.Once);
    }

    [Fact]
    public async Task FlushAsync()
    {
        // Arrange
        using var tcs = new CancellationTokenSource();
        var contextCore = new Mock<HttpContextCore>();
        contextCore.Setup(s => s.RequestAborted).Returns(tcs.Token);

        var responseCore = new Mock<HttpResponseCore>();
        var stream = new Mock<Stream>();
        responseCore.Setup(r => r.Body).Returns(stream.Object);
        responseCore.Setup(r => r.HttpContext).Returns(contextCore.Object);

        var response = new HttpResponse(responseCore.Object);

        // Act
        await response.FlushAsync();

        // Assert
        stream.Verify(s => s.FlushAsync(tcs.Token), Times.Once);
    }

    [Fact]
    public void ContentType()
    {
        // Arrange
        var headers = new HeaderDictionary();

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.Headers).Returns(headers);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.ContentType = "application/json";

        // Assert
        Assert.Equal("application/json", headers[HeaderNames.ContentType]);
    }

    [Fact]
    public void ContentTypeWithEncoding()
    {
        // Arrange
        var headers = new HeaderDictionary();

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.Headers).Returns(headers);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.ContentType = "application/json";
        response.ContentEncoding = Encoding.UTF32;

        // Assert
        Assert.Equal("application/json; charset=utf-32", headers[HeaderNames.ContentType]);
    }

    [Fact]
    public void ContentTypeWithCharset()
    {
        // Arrange
        var headers = new HeaderDictionary();

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.Headers).Returns(headers);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.ContentType = "application/json";
        response.Charset = Encoding.UTF32.WebName;

        // Assert
        Assert.Equal("application/json; charset=utf-32", headers[HeaderNames.ContentType]);
    }

    [Fact]
    public void Headers()
    {
        // Arrange
        var headersCore = new HeaderDictionary();

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.Headers).Returns(headersCore);

        var response = new HttpResponse(responseCore.Object);

        // Act
        var headers1 = response.Headers;
        var headers2 = response.Headers;

        // Assert
        Assert.Same(headers1, headers2);
        Assert.IsType<StringValuesDictionaryNameValueCollection>(headers1);
    }

    [Fact]
    public void Clear()
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            { HeaderNames.ContentType, "application/json" },
        };

        var feature = new Mock<IHttpResponseBufferingFeature>();
        var responseFeature = new Mock<IHttpResponseFeature>();

        var features = new Mock<IFeatureCollection>();
        features.Setup(f => f.Get<IHttpResponseBufferingFeature>()).Returns(feature.Object);
        features.Setup(f => f.Get<IHttpResponseFeature>()).Returns(responseFeature.Object);

        var body = new Mock<Stream>();

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features.Object);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);
        responseCore.SetupProperty(r => r.StatusCode);
        responseCore.Setup(r => r.Body).Returns(body.Object);
        responseCore.Setup(r => r.Headers).Returns(headers);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.Clear();

        // Assert
        feature.Verify(f => f.ClearContent(), Times.Once);
        Assert.Empty(headers);
    }

    [Fact]
    public void ClearContentsStreamNotSeekable()
    {
        // Arrange
        var feature = new Mock<IHttpResponseBufferingFeature>();
        var body = new Mock<Stream>();

        var features = new Mock<IFeatureCollection>();
        features.Setup(f => f.Get<IHttpResponseBufferingFeature>()).Returns(feature.Object);

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features.Object);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);
        responseCore.Setup(r => r.Body).Returns(body.Object);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.ClearContent();

        // Assert
        feature.Verify(f => f.ClearContent(), Times.Once);
    }

    [Fact]
    public void ClearContentsStreamSeekable()
    {
        // Arrange
        var body = new Mock<Stream>();
        body.Setup(b => b.CanSeek).Returns(true);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.Body).Returns(body.Object);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.ClearContent();

        // Assert
        body.Verify(b => b.SetLength(0), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsClientConnected(bool isConnected)
    {
        // Arrange
        var context = new Mock<HttpContextCore>();
        context.SetupProperty(c => c.RequestAborted);
        using var cts = new CancellationTokenSource();

        if (!isConnected)
        {
            cts.Cancel();
        }

        context.Object.RequestAborted = cts.Token;

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);

        var response = new HttpResponse(responseCore.Object);

        // Act
        var result = response.IsClientConnected;

        // Assert
        Assert.Equal(isConnected, result);
    }

    [Fact]
    public void Cookies()
    {
        // Arrange
        var cookies = new Mock<IResponseCookies>();

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.Cookies).Returns(cookies.Object);

        var response = new HttpResponse(responseCore.Object);

        // Act
        var cookies1 = response.Cookies;
        var cookies2 = response.Cookies;

        // Assert
        Assert.Same(cookies1, cookies2);
    }

    [Fact]
    public void ClearHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var response = new HttpResponse(context.Response)
        {
            StatusCode = _fixture.Create<int>(),
            SubStatusCode = _fixture.Create<int>(),
            StatusDescription = _fixture.Create<string>(),
            ContentType = "application/json",
        };

        response.Headers.Add(_fixture.Create<string>(), _fixture.Create<string>());
        response.Cookies.Add(new(_fixture.Create<string>()));

        // Ensure IsRequestBeingRedirected is set to true
        response.RedirectPermanent(_fixture.Create<string>(), false);

        // Act
        response.ClearHeaders();

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal(0, response.SubStatusCode);
        Assert.Equal("OK", response.StatusDescription);
        Assert.Equal("text/html", response.ContentType);
        Assert.False(response.IsRequestBeingRedirected);
        Assert.Equal(Encoding.UTF8.WebName, response.Charset);
        Assert.Empty(response.Cookies);
        Assert.Collection(context.Response.Headers, h =>
        {
            Assert.Equal(HeaderNames.ContentType, h.Key);
            Assert.Equal("text/html; charset=utf-8", h.Value);
        });
    }

    [Fact]
    public void WriteFile()
        => SendFileTest((response, file) => response.WriteFile(file));

    [Fact]
    public void TransmitFile()
        => SendFileTest((response, file) => response.TransmitFile(file));

    [Fact]
    public void TransmitFileArgs()
        => SendFileTest((response, file, offset, length) => response.TransmitFile(file, offset, length!.Value), 30, 3);

    private static void SendFileTest(Action<HttpResponse, string> action)
        => SendFileTest((response, file, offset, length) => action(response, file), 0, default);

    private static void SendFileTest(Action<HttpResponse, string, long, long?> action, long offset, long? length)
    {
        // Arrange
        const string FileName = "somefile.txt";

        var responsebody = new Mock<IHttpResponseBodyFeature>();

        var features = new Mock<IFeatureCollection>();
        features.Setup(f => f.Get<IHttpResponseBodyFeature>()).Returns(responsebody.Object);

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features.Object);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);

        var response = new HttpResponse(responseCore.Object);

        // Act
        action(response, FileName, offset, length);

        // Assert
        responsebody.Verify(r => r.SendFileAsync(FileName, offset, length, default), Times.Once);
    }

    [InlineData(200, false)]
    [InlineData(299, false)]
    [InlineData(300, true)]
    [InlineData(301, true)]
    [InlineData(399, true)]
    [InlineData(400, false)]
    [Theory]
    public void IsRequestBeingRedirected(int statusCode, bool isRequestBeingRedirected)
    {
        // Arrange
        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.StatusCode).Returns(statusCode);

        var response = new HttpResponse(responseCore.Object);

        // Act
        var result = response.IsRequestBeingRedirected;

        // Assert
        Assert.Equal(isRequestBeingRedirected, result);
    }

    [InlineData("/", "~", "/", true, null)]
    [InlineData("/", "~", "/", true, false)]
    [InlineData("/", "~", "/", true, true)]
    [InlineData("/", "~", "/", false, null)]
    [InlineData("/", "~", "/", false, false)]
    [InlineData("/", "~", "/", false, true)]

    [InlineData("/", "~/dir", "/dir", true, null)]
    [InlineData("/", "~/dir", "/dir", true, false)]
    [InlineData("/", "~/dir", "/dir", true, true)]
    [InlineData("/", "~/dir", "/dir", false, null)]
    [InlineData("/", "~/dir", "/dir", false, false)]
    [InlineData("/", "~/dir", "/dir", false, true)]

    [InlineData("/", "/dir", "/dir", true, null)]
    [InlineData("/", "/dir", "/dir", true, false)]
    [InlineData("/", "/dir", "/dir", true, true)]
    [InlineData("/", "/dir", "/dir", false, null)]
    [InlineData("/", "/dir", "/dir", false, false)]
    [InlineData("/", "/dir", "/dir", false, true)]

    [InlineData("/dir1/", "/dir2", "/dir2", true, null)]
    [InlineData("/dir1/", "/dir2", "/dir2", true, false)]
    [InlineData("/dir1/", "/dir2", "/dir2", true, true)]
    [InlineData("/dir1/", "/dir2", "/dir2", false, null)]
    [InlineData("/dir1/", "/dir2", "/dir2", false, false)]
    [InlineData("/dir1/", "/dir2", "/dir2", false, true)]

    [InlineData("/dir1/", "~/dir2", "/dir1/dir2", true, null)]
    [InlineData("/dir1/", "~/dir2", "/dir1/dir2", true, false)]
    [InlineData("/dir1/", "~/dir2", "/dir1/dir2", true, true)]
    [InlineData("/dir1/", "~/dir2", "/dir1/dir2", false, null)]
    [InlineData("/dir1/", "~/dir2", "/dir1/dir2", false, false)]
    [InlineData("/dir1/", "~/dir2", "/dir1/dir2", false, true)]

    [InlineData("/dir1/", "", "/", true, null)]
    [InlineData("/dir1/", "", "/", true, false)]
    [InlineData("/dir1/", "", "/", true, true)]
    [InlineData("/dir1/", "", "/", false, null)]
    [InlineData("/dir1/", "", "/", false, false)]
    [InlineData("/dir1/", "", "/", false, true)]

    [Theory]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "Testing")]
    public void Redirect(string vdir, string url, string resolved, bool permanent, bool? endResponse)
    {
        // Arrange
        var isEndCalled = endResponse ?? true;

        var runtime = new Mock<IHttpRuntime>();
        runtime.Setup(r => r.AppDomainAppVirtualPath).Returns(vdir);

        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetService(typeof(IHttpRuntime))).Returns(runtime.Object);

        var endFeature = new Mock<IHttpResponseEndFeature>();
        endFeature.SetupAllProperties();

        var context = new DefaultHttpContext();
        context.Features.Set(endFeature.Object);
        context.RequestServices = services.Object;

        var response = new HttpResponse(context.Response);

        // Act
        if (endResponse.HasValue)
        {
            if (permanent)
            {
                response.RedirectPermanent(url, endResponse.Value);
            }
            else
            {
                response.Redirect(url, endResponse.Value);
            }
        }
        else
        {
            if (permanent)
            {
                response.RedirectPermanent(url);
            }
            else
            {
                response.Redirect(url);
            }
        }

        // Assert
        Assert.Equal(resolved, response.RedirectLocation);
        Assert.Null(context.Features.GetRequired<IHttpResponseFeature>().ReasonPhrase);
        Assert.Equal(2, context.Response.Headers.Count);
        Assert.Equal(resolved, context.Response.Headers.Location);
        Assert.Equal("text/html", context.Response.Headers.ContentType);
        Assert.Equal(permanent ? 301 : 302, context.Response.StatusCode);

        endFeature.Verify(b => b.EndAsync(), isEndCalled ? Times.Once : Times.Never);
    }
}
