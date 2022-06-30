// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Web.Configuration;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests.Configuration;

public class BrowserCapabilitiesFactoryTests
{
    [MemberData(nameof(UserAgentTestData))]
    [Theory]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Used for tests")]
    public void VerifyCapabilities(TestData data)
    {
        // Arrange
        var browserFactory = new BrowserCapabilitiesFactory();

        // Act
        var result = browserFactory.Process(data.UserAgent);

        // Assert
        Assert.Equal(data.Browser, result["browser"]);
        Assert.Equal(data.Version, result["version"]);
        Assert.Equal(data.MajorVersion, result["majorversion"]);
        Assert.Equal(data.MinorVersion, result["minorversion"]);
        Assert.Equal(data.Platform, result["platform"]);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Used for tests")]
    public record TestData(string UserAgent)
    {
        public string? Browser { get; init; }

        public string? Version { get; init; }

        public string? MajorVersion { get; init; }

        public string? MinorVersion { get; init; }

        public string? Platform { get; init; }

        public string? Crawler { get; init; }

        public override string ToString() => UserAgent;
    }

    public static IEnumerable<object[]> UserAgentTestData
    {
        get
        {
            yield return new object[]
            {
                    new TestData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.63 Safari/537.36")
                    {
                        Browser = "Chrome",
                        Version = "102.0",
                        MajorVersion = "102",
                        MinorVersion = "0",
                        Platform = "WinNT",
                    }
            };

            yield return new object[]
            {
                    new TestData("Mozilla/5.0 (Macintosh; Intel Mac OS X 12_4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.63 Safari/537.36")
                    {
                        Browser = "Chrome",
                        Version = "102.0",
                        MajorVersion = "102",
                        MinorVersion = "0",
                        Platform = "Unknown",
                    }
            };

            yield return new object[]
            {
                    new TestData("Mozilla/5.0 (iPhone; CPU iPhone OS 15_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/102.0.5005.87 Mobile/15E148 Safari/604.1")
                    {
                        Browser = "Safari",
                        Version = "0.0",
                        MajorVersion = "0",
                        MinorVersion = "0",
                        Platform = "Unknown",
                    }
            };

            yield return new object[]
            {
                    new TestData("Mozilla/5.0 (iPad; CPU OS 15_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/102.0.5005.87 Mobile/15E148 Safari/604.1")
                    {
                        Browser = "Safari",
                        Version = "0.0",
                        MajorVersion = "0",
                        MinorVersion = "0",
                        Platform = "Unknown",
                    }
            };

            yield return new object[]
            {
                    new TestData("Mozilla/5.0 (iPod; CPU iPhone OS 15_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/102.0.5005.87 Mobile/15E148 Safari/604.1")
                    {
                        Browser = "Safari",
                        Version = "0.0",
                        MajorVersion = "0",
                        MinorVersion = "0",
                        Platform = "Unknown",
                    }
            };

            yield return new object[]
            {
                    new TestData("Mozilla/5.0 (Linux; Android 10) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.99 Mobile Safari/537.36")
                    {
                        Browser = "Chrome",
                        Version = "102.0",
                        MajorVersion = "102",
                        MinorVersion = "0",
                        Platform = "Unknown",
                    }
            };

            yield return new object[]
            {
                    new TestData("Mozilla/5.0 (Linux; Android 10; SM-A102U) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.99 Mobile Safari/537.36")
                    {
                        Browser = "Chrome",
                        Version = "102.0",
                        MajorVersion = "102",
                        MinorVersion = "0",
                        Platform = "Unknown",
                    }
            };

            // Firefox
            yield return new object[]
            {
                    new TestData("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:53.0) Gecko/20100101 Firefox/53.0")
                    {
                        Browser = "Firefox",
                        Version = "53.0",
                        MajorVersion = "53",
                        MinorVersion = "0",
                        Platform = "WinNT",
                    }
            };

            yield return new object[]
            {
                    new TestData("Mozilla/5.0 (Macintosh; Intel Mac OS X 12.4; rv:101.0) Gecko/20100101 Firefox/101.0")
                    {
                        Browser = "Firefox",
                        Version = "101.0",
                        MajorVersion = "101",
                        MinorVersion = "0",
                        Platform = "Unknown",
                    }
            };

            yield return new object[]
            {
                    new TestData("Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.0; Trident/5.0; Trident/5.0)")
                    {
                        Browser = "IE",
                        Version = "9.0",
                        MajorVersion = "9",
                        MinorVersion = "0",
                        Platform = "WinNT",
                    }
            };
            yield return new object[]
            {
                    new TestData("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0; MDDCJS)")
                    {
                        Browser = "IE",
                        Version = "10.0",
                        MajorVersion = "10",
                        MinorVersion = "0",
                        Platform = "WinNT",
                    }
            };

            yield return new object[]
            {
                    new TestData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.79 Safari/537.36 Edge/14.14393")
                    {
                        Browser = "Chrome",
                        Version = "51.0",
                        MajorVersion = "51",
                        MinorVersion = "0",
                        Platform = "WinNT",
                    }
            };

            yield return new object[]
            {
                    new TestData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.63 Safari/537.36 OPR/87.0.4390.36")
                    {
                        Browser = "Chrome",
                        Version = "102.0",
                        MajorVersion = "102",
                        MinorVersion = "0",
                        Platform = "WinNT",
                    }
            };
        }
    }
}
