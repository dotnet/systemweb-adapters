// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Caching;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class HttpServerUtilityTests
{
    private readonly Fixture _fixture;

    public HttpServerUtilityTests()
    {
        _fixture = new Fixture();
        HttpRuntime.Current = new TestHttpRuntime();
    }

    internal sealed class TestHttpRuntime : IHttpRuntime
    {
        private Cache? _cache;

        public string AppDomainAppVirtualPath => "/";

        public string AppDomainAppPath => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            "C:\\ExampleSites\\TestMapPath" : "/apps/test-map-path";

        public Cache Cache => _cache ??= new Cache();
    }

    [Fact]
    public void UrlTokenEncodeEmpty()
    {
        // Act
        var encoded = HttpServerUtility.UrlTokenDecode(string.Empty);

        // Assert
        Assert.Same(Array.Empty<byte>(), encoded);
    }

    [Fact]
    public void UrlTokenNull()
    {
        Assert.Throws<ArgumentNullException>(() => HttpServerUtility.UrlTokenDecode(null!));
        Assert.Throws<ArgumentNullException>(() => HttpServerUtility.UrlTokenEncode(null!));
    }

    [InlineData('9' + 1)]
    [InlineData('0' - 1)]
    [Theory]
    public void UrlTokenDecodeInvalidPaddingCount(char padChar)
    {
        // Arrange
        var input = "aa" + padChar;

        // Act
        var result = HttpServerUtility.UrlTokenDecode(input);

        // Assert
        Assert.Null(result);
    }

    [InlineData(new byte[] { 185, 178, 254 }, "ubL-0")] // base64 contains +
    [InlineData(new byte[] { 253, 7, 171 }, "_Qer0")] // base64 contains /
    [InlineData(new byte[] { 211, 90, 167, 128, 197 }, "01qngMU1")] // base64 contains padding
    [Theory]
    public void UrlTokenRoundtripBytes(byte[] bytes, string expected)
    {
        Assert.Equal(expected, HttpServerUtility.UrlTokenEncode(bytes));
        Assert.Equal(bytes, HttpServerUtility.UrlTokenDecode(expected));
    }

    [InlineData("", "")]
    [InlineData("a", "YQ2")]
    [InlineData("j~", "an41")]
    [Theory]
    public void UrlTokenRoundtrip(string input, string expected)
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var encoded = HttpServerUtility.UrlTokenEncode(bytes);
        var decoded = HttpServerUtility.UrlTokenDecode(expected);

        // Assert
        Assert.Equal(expected, encoded);
        Assert.Equal(bytes, decoded);
    }

    // Test data from https://docs.microsoft.com/en-us/dotnet/api/system.web.httpserverutility.mappath?view=netframework-4.8
    [InlineData("/RootLevelPage.aspx",null,"")]
    [InlineData("/RootLevelPage.aspx", "", "")]
    [InlineData("/RootLevelPage.aspx", "/DownOneLevel/DownLevelPage.aspx", "DownOneLevel","DownLevelPage.aspx")]
    [InlineData("/RootLevelPage.aspx", "/NotRealFolder", "NotRealFolder")]
    [InlineData("/DownOneLevel/DownLevelPage.aspx", null, "DownOneLevel")]
    [InlineData("/DownOneLevel/DownLevelPage.aspx", "../RootLevelPage.aspx", "RootLevelPage.aspx")]
    [InlineData("/api/test/request/info", null, "api","test","request")]
    [InlineData("/api/test/request/info", "", "api", "test", "request")]
    [InlineData("/api/test/request/info", "/UploadedFiles", "UploadedFiles")]
    [InlineData("/api/test/request/info", "UploadedFiles", "api", "test", "request", "UploadedFiles")]
    [InlineData("/api/test/request/info", "~/MyUploadedFiles", "MyUploadedFiles")]
    [Theory]
    public void MapPath(string page, string? path, params string[] segments)
    {
        // Arrange
        var coreContext = new Mock<HttpContextCore>();
        var coreRequest = new Mock<HttpRequestCore>();
        coreRequest.Setup(c => c.Path).Returns(page);
        coreContext.Setup(c => c.Request).Returns(coreRequest.Object);

        var context = new HttpContext(coreContext.Object);

        // Act
        var result = context.Server.MapPath(path);

        var relative = System.IO.Path.Combine(segments);
        var expected = System.IO.Path.Combine(HttpRuntime.Current.AppDomainAppPath, relative);

        // Assert
        Assert.Equal(expected, result);
    }

    // Test data from https://docs.microsoft.com/en-us/dotnet/api/system.web.httpserverutility.mappath?view=netframework-4.8
    [InlineData("/RootLevelPage.aspx", "../OutsideApplication", typeof(HttpException))]
    [InlineData("/RootLevelPage.aspx", "C:\\OutsideApplication", typeof(HttpException))]
    [InlineData("/RootLevelPage.aspx", "\\\\SomeServer\\Share\\Path", typeof(HttpException))]
    [Theory]
    public void MapPathException(string page, string? path, Type expected)
    {
        // Arrange
        var coreContext = new Mock<HttpContextCore>();
        var coreRequest = new Mock<HttpRequestCore>();
        coreRequest.Setup(c => c.Path).Returns(page);
        coreContext.Setup(c => c.Request).Returns(coreRequest.Object);

        var context = new HttpContext(coreContext.Object);

        // Assert
        Assert.Throws(expected, ()=> context.Server.MapPath(path));
    }
}
