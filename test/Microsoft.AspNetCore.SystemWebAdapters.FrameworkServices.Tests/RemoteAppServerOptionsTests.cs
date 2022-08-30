// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class RemoteAppServerOptionsTests
{
    [InlineData("HeaderName", "36705d36-eba0-44f9-a2e0-c57e6f521274", true)]
    [InlineData("HeaderName", "{36705d36eba044f9a2e0c57e6f521274}", true)]
    [InlineData("HeaderName", "00000000-0000-0000-0000-000000000000", false)]
    [InlineData(null, "36705d36-eba0-44f9-a2e0-c57e6f521274", false)]
    [InlineData("", "36705d36-eba0-44f9-a2e0-c57e6f521274", false)]
    [InlineData("HeaderName", "MyKey", false)]
    [InlineData(null, "MyKey", false)]
    [InlineData("", "MyKey", false)]
    [InlineData("HeaderName", null, false)]
    [InlineData("HeaderName", "", false)]
    [Theory]
    public void VerifyIsCalled(string apiKeyHeader, string apiKey, bool shouldSucceed)
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TestBuilder { Services = services };

        builder.AddRemoteAppServer(remote => remote
            .Configure(options =>
            {
                options.ApiKey = apiKey;
                options.ApiKeyHeader = apiKeyHeader;
            }));

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<RemoteAppServerOptions>>();

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
