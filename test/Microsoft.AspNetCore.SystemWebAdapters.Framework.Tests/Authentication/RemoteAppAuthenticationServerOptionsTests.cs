// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class RemoteAppAuthenticationServerOptionsTests
{
    [InlineData("AuthEndpoint", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [Theory]
    public void VerifyIsCalled(string endpoint, bool shouldSucceed)
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TestBuilder { Services = services };

        builder.AddRemoteAppServerAuthentication(options =>
        {
            options.AuthenticationEndpointPath = endpoint;
        });

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<RemoteAppAuthenticationServerOptions>>();

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

    private class TestBuilder : ISystemWebAdapterRemoteAppBuilder
    {
        public IServiceCollection Services { get; set; } = null!;
    }
}
