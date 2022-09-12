// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using System.Web;
using System.Security.Policy;
using System;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class RemoteAppClientOptionsTests
{
    [InlineData("HeaderName", "36705d36-eba0-44f9-a2e0-c57e6f521274", "http://test", true)]
    [InlineData("HeaderName", "{36705d36eba044f9a2e0c57e6f521274}", "http://test", true)]
    [InlineData("HeaderName", "00000000-0000-0000-0000-000000000000", "http://test", false)]
    [InlineData(null, "36705d36-eba0-44f9-a2e0-c57e6f521274", "http://test", false)]
    [InlineData("", "36705d36-eba0-44f9-a2e0-c57e6f521274", "http://test", false)]
    [InlineData("HeaderName", "MyKey", "http://test", false)]
    [InlineData(null, "MyKey", "http://test", false)]
    [InlineData("", "MyKey", "http://test", false)]
    [InlineData("HeaderName", null, "http://test", false)]
    [InlineData("HeaderName", "", "http://test", false)]
    [InlineData("HeaderName", "36705d36-eba0-44f9-a2e0-c57e6f521274", null, false)]
    [Theory]
    public void VerifyIsCalled(string apiKeyHeader, string apiKey, string remoteAppUrl, bool shouldSucceed)
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TestBuilder { Services = services };

        builder.AddRemoteAppClient(options =>
        {
            options.ApiKey = apiKey;
            options.ApiKeyHeader = apiKeyHeader;

            if (remoteAppUrl is not null)
            {
                options.RemoteAppUrl = new Uri(remoteAppUrl, UriKind.Absolute);
            }
        });

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<RemoteAppClientOptions>>();

        // Act/Assert
        if (shouldSucceed)
        {
            _ = options.Value;
        }
        else
        {
            Assert.Throws<OptionsValidationException>(() => options.Value);
        }
    }

    private class TestBuilder : ISystemWebAdapterBuilder
    {
        public IServiceCollection Services { get; set; } = null!;
    }
}
