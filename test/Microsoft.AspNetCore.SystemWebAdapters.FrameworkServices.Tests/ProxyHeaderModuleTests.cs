// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class ProxyHeaderModuleTests
{
    private const string ForwardedHost = "x-forwarded-host";
    private const string ForwardedProto = "x-forwarded-proto";
    private const string ForwardedFor = "x-forwarded-for";
    private const string Host = "host";
    private const string RemoteAddress = "REMOTE_ADDR";
    private const string RemoteHost = "REMOTE_HOST";
    private const string ServerName = "SERVER_NAME";
    private const string ServerPort = "SERVER_PORT";
    private const string ServerHttps = "HTTPS";
    private const string On = "on";
    private const string Off = "off";

    [Fact]
    public void NoHeaderChange()
    {
        // Arrange
        var requestHeaders = new NameValueCollection();
        var serverVariables = new NameValueCollection();
        var options = new ProxyOptions();
        var module = new ProxyHeaderModule(Options.Create(options));

        // Act
        module.UseHeaders(requestHeaders, serverVariables);

        // Assert
        Assert.Empty(requestHeaders);
        Assert.Empty(serverVariables);
    }

    [Fact]
    public void HostWithPortNoProto()
    {
        // Arrange
        var requestHeaders = new NameValueCollection
        {
            { ForwardedHost, "localhost:81" }
        };
        var serverVariables = new NameValueCollection();
        var options = new ProxyOptions();
        var module = new ProxyHeaderModule(Options.Create(options));

        // Act
        module.UseHeaders(requestHeaders, serverVariables);

        // Assert
        Assert.Equal("localhost", serverVariables[ServerName]);
        Assert.Equal("81", serverVariables[ServerPort]);
        Assert.Null(serverVariables[RemoteAddress]);
        Assert.Null(serverVariables[RemoteHost]);
        Assert.Equal("localhost:81", requestHeaders[Host]);
        Assert.Null(requestHeaders[options.OriginalHostHeaderName]);
        Assert.Equal(Off, serverVariables[ServerHttps]);
    }

    [Fact]
    public void HostWithNoPortNoProto()
    {
        // Arrange
        var requestHeaders = new NameValueCollection
        {
            { ForwardedHost, "localhost" }
        };
        var serverVariables = new NameValueCollection();
        var options = new ProxyOptions();
        var module = new ProxyHeaderModule(Options.Create(options));

        // Act
        module.UseHeaders(requestHeaders, serverVariables);

        // Assert
        Assert.Equal("localhost", serverVariables[ServerName]);
        Assert.Equal("80", serverVariables[ServerPort]);
        Assert.Null(serverVariables[RemoteAddress]);
        Assert.Null(serverVariables[RemoteHost]);
        Assert.Equal("localhost", requestHeaders[Host]);
        Assert.Null(requestHeaders[options.OriginalHostHeaderName]);
        Assert.Equal(Off, serverVariables[ServerHttps]);
    }

    [Fact]
    public void HostWithNoPortHttp()
    {
        // Arrange
        var requestHeaders = new NameValueCollection
        {
            { ForwardedHost, "localhost" },
            { ForwardedProto, "http" }
        };
        var serverVariables = new NameValueCollection();
        var options = new ProxyOptions();
        var module = new ProxyHeaderModule(Options.Create(options));

        // Act
        module.UseHeaders(requestHeaders, serverVariables);

        // Assert
        Assert.Equal("localhost", serverVariables[ServerName]);
        Assert.Equal("80", serverVariables[ServerPort]);
        Assert.Null(serverVariables[RemoteAddress]);
        Assert.Null(serverVariables[RemoteHost]);
        Assert.Equal("localhost", requestHeaders[Host]);
        Assert.Null(requestHeaders[options.OriginalHostHeaderName]);
        Assert.Equal(Off, serverVariables[ServerHttps]);
    }

    [Fact]
    public void HostAlreadySet()
    {
        // Arrange
        var requestHeaders = new NameValueCollection
        {
            { Host, "localhost2:90" },
            { ForwardedHost, "localhost" },
            { ForwardedProto, "http" }
        };
        var serverVariables = new NameValueCollection();
        var options = new ProxyOptions();
        var module = new ProxyHeaderModule(Options.Create(options));

        // Act
        module.UseHeaders(requestHeaders, serverVariables);

        // Assert
        Assert.Equal("localhost", serverVariables[ServerName]);
        Assert.Equal("80", serverVariables[ServerPort]);
        Assert.Null(serverVariables[RemoteAddress]);
        Assert.Null(serverVariables[RemoteHost]);
        Assert.Equal("localhost", requestHeaders[Host]);
        Assert.Equal("localhost2:90", requestHeaders[options.OriginalHostHeaderName]);
        Assert.Equal(Off, serverVariables[ServerHttps]);
    }

    [Fact]
    public void HostWithNoPortHttps()
    {
        // Arrange
        var requestHeaders = new NameValueCollection
        {
            { ForwardedHost, "localhost" },
            { ForwardedProto, "https" }
        };
        var serverVariables = new NameValueCollection();
        var options = new ProxyOptions();
        var module = new ProxyHeaderModule(Options.Create(options));

        // Act
        module.UseHeaders(requestHeaders, serverVariables);

        // Assert
        Assert.Equal("localhost", serverVariables[ServerName]);
        Assert.Equal("443", serverVariables[ServerPort]);
        Assert.Null(serverVariables[RemoteAddress]);
        Assert.Null(serverVariables[RemoteHost]);
        Assert.Equal("localhost", requestHeaders[Host]);
        Assert.Null(requestHeaders[options.OriginalHostHeaderName]);
        Assert.Equal(On, serverVariables[ServerHttps]);
    }

    [Fact]
    public void IPv6NoPort()
    {
        // Arrange
        var requestHeaders = new NameValueCollection
        {
            { ForwardedHost, "::1" },
            { ForwardedProto, "https" }
        };
        var serverVariables = new NameValueCollection();
        var options = new ProxyOptions();
        var module = new ProxyHeaderModule(Options.Create(options));

        // Act
        module.UseHeaders(requestHeaders, serverVariables);

        // Assert
        Assert.Equal("[::1]", serverVariables[ServerName]);
        Assert.Equal("443", serverVariables[ServerPort]);
        Assert.Null(serverVariables[RemoteAddress]);
        Assert.Null(serverVariables[RemoteHost]);
        Assert.Equal("::1", requestHeaders[Host]);
        Assert.Null(requestHeaders[options.OriginalHostHeaderName]);
        Assert.Equal(On, serverVariables[ServerHttps]);
    }

    [Fact]
    public void IPv6WithPort()
    {
        // Arrange
        var requestHeaders = new NameValueCollection
        {
            { ForwardedHost, "[::1]:81" },
            { ForwardedProto, "https" }
        };
        var serverVariables = new NameValueCollection();
        var options = new ProxyOptions();
        var module = new ProxyHeaderModule(Options.Create(options));

        // Act
        module.UseHeaders(requestHeaders, serverVariables);

        // Assert
        Assert.Equal("[::1]", serverVariables[ServerName]);
        Assert.Equal("81", serverVariables[ServerPort]);
        Assert.Null(serverVariables[RemoteAddress]);
        Assert.Null(serverVariables[RemoteHost]);
        Assert.Equal("[::1]:81", requestHeaders[Host]);
        Assert.Null(requestHeaders[options.OriginalHostHeaderName]);
        Assert.Equal(On, serverVariables[ServerHttps]);
    }

    [Fact]
    public void ForwardedForSet()
    {
        const string ForwardedForValue = "something";

        // Arrange
        var requestHeaders = new NameValueCollection
        {
            { ForwardedFor, ForwardedForValue },
        };
        var serverVariables = new NameValueCollection();
        var options = new ProxyOptions();
        var module = new ProxyHeaderModule(Options.Create(options));

        // Act
        module.UseHeaders(requestHeaders, serverVariables);

        // Assert
        Assert.Null(requestHeaders[Host]);
        Assert.Null(serverVariables[ServerName]);
        Assert.Null(serverVariables[ServerPort]);
        Assert.Null(serverVariables[ServerHttps]);
        Assert.Equal(ForwardedForValue, serverVariables[RemoteAddress]);
        Assert.Equal(ForwardedForValue, serverVariables[RemoteHost]);
        Assert.Null(requestHeaders[options.OriginalHostHeaderName]);
    }

    [Fact]
    public void ForwardedForPreservesOriginalRemoteAddr()
    {
        const string ForwardedForValue = "10.0.0.1";
        const string OriginalRemoteAddr = "192.168.1.100";

        // Arrange
        var requestHeaders = new NameValueCollection
        {
            { ForwardedFor, ForwardedForValue },
        };
        var serverVariables = new NameValueCollection
        {
            { RemoteAddress, OriginalRemoteAddr },
        };
        var options = new ProxyOptions();
        var module = new ProxyHeaderModule(Options.Create(options));

        // Act
        module.UseHeaders(requestHeaders, serverVariables);

        // Assert
        Assert.Equal(ForwardedForValue, serverVariables[RemoteAddress]);
        Assert.Equal(ForwardedForValue, serverVariables[RemoteHost]);
        Assert.Equal(OriginalRemoteAddr, requestHeaders[options.OriginalForHeaderName]);
    }

    [Fact]
    public void ForwardedForPreservesOriginalRemoteAddrWithCustomHeaderName()
    {
        const string ForwardedForValue = "10.0.0.1";
        const string OriginalRemoteAddr = "192.168.1.100";
        const string CustomHeaderName = "X-Custom-Original-For";

        // Arrange
        var requestHeaders = new NameValueCollection
        {
            { ForwardedFor, ForwardedForValue },
        };
        var serverVariables = new NameValueCollection
        {
            { RemoteAddress, OriginalRemoteAddr },
        };
        var options = new ProxyOptions
        {
            OriginalForHeaderName = CustomHeaderName
        };
        var module = new ProxyHeaderModule(Options.Create(options));

        // Act
        module.UseHeaders(requestHeaders, serverVariables);

        // Assert
        Assert.Equal(ForwardedForValue, serverVariables[RemoteAddress]);
        Assert.Equal(ForwardedForValue, serverVariables[RemoteHost]);
        Assert.Equal(OriginalRemoteAddr, requestHeaders[CustomHeaderName]);
    }

    [Fact]
    public void ForwardedForDoesNotSetOriginalWhenNoRemoteAddr()
    {
        const string ForwardedForValue = "10.0.0.1";

        // Arrange
        var requestHeaders = new NameValueCollection
        {
            { ForwardedFor, ForwardedForValue },
        };
        var serverVariables = new NameValueCollection();
        var options = new ProxyOptions();
        var module = new ProxyHeaderModule(Options.Create(options));

        // Act
        module.UseHeaders(requestHeaders, serverVariables);

        // Assert
        Assert.Equal(ForwardedForValue, serverVariables[RemoteAddress]);
        Assert.Equal(ForwardedForValue, serverVariables[RemoteHost]);
        Assert.Null(requestHeaders[options.OriginalForHeaderName]);
    }
}
