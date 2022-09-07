// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;
using System.Web;
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
        HttpRuntime.Current = new DefaultHttpRuntime();
    }

    internal class DefaultHttpRuntime : IHttpRuntime
    {
        public string AppDomainAppVirtualPath => "/";
        public string AppDomainAppPath => "C:\\ExampleSites\\TestMapPath";
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
    [InlineData("/RootLevelPage.aspx",null,"C:\\ExampleSites\\TestMapPath")]
    [InlineData("/RootLevelPage.aspx", "/DownOneLevel/DownLevelPage.aspx", "C:\\ExampleSites\\TestMapPath\\DownOneLevel\\DownLevelPage.aspx")]
    [InlineData("/RootLevelPage.aspx", "/NotRealFolder", "C:\\ExampleSites\\TestMapPath\\NotRealFolder")]
    [InlineData("/DownOneLevel/DownLevelPage.aspx", null, "C:\\ExampleSites\\TestMapPath\\DownOneLevel")]
    [InlineData("/DownOneLevel/DownLevelPage.aspx", "../RootLevelPage.aspx", "C:\\ExampleSites\\TestMapPath\\RootLevelPage.aspx")]
    [InlineData("/api/test/request/info", "/UploadedFiles", "C:\\ExampleSites\\TestMapPath\\UploadedFiles")]
    [InlineData("/api/test/request/info", "UploadedFiles", "C:\\ExampleSites\\TestMapPath\\api\\test\\request\\UploadedFiles")]
    [InlineData("/api/test/request/info", "~/MyUploadedFiles", "C:\\ExampleSites\\TestMapPath\\MyUploadedFiles")]
    [Theory]
    public void MapPath(string page, string? path, string expected)
    {
        // Arrange
        var coreContext = new Mock<HttpContextCore>();
        var coreRequest = new Mock<HttpRequestCore>();
        coreRequest.Setup(c => c.Path).Returns(page);
        coreContext.Setup(c => c.Request).Returns(coreRequest.Object);

        var context = new HttpContext(coreContext.Object);

        // Act
        var result = context.Server.MapPath(path);

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
