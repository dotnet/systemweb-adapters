// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class RemoteAppOptionsTests
{
    [InlineData("HeaderName", "MyKey", true)]
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
