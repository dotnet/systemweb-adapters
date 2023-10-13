// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Web.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests;

[Collection(nameof(SelfHostedTests))]
public class MachineKeyTests
{
    private static readonly byte[] protectedData = new byte[] { 1, 2, 3 };
    private static readonly byte[] plaintext = new byte[] { 5, 6 };

    [Fact]
    public void Protect()
    {
        // Arrange
        var provider = BuildProvider();
        using var host = StartHost(provider);

        // Act
        var result = MachineKey.Protect(plaintext);

        // Assert
        Assert.Equal(protectedData, result);
    }

    [InlineData("purpose1")]
    [InlineData("purpose1", "purpose2")]
    [InlineData("purpose1", "purpose2", "purpose3")]
    [Theory]
    public void ProtectWithPurposes(params string[] purposes)
    {
        ArgumentNullException.ThrowIfNull(purposes);

        // Arrange
        var provider = BuildProvider(purposes);
        using var host = StartHost(provider);

        // Act
        var result = MachineKey.Protect(plaintext, purposes);

        // Assert
        Assert.Equal(protectedData, result);
    }

    [Fact]
    public void Unprotect()
    {
        // Arrange
        var provider = BuildProvider();
        using var host = StartHost(provider);

        // Act
        var result = MachineKey.Unprotect(protectedData);

        // Assert
        Assert.Equal(plaintext, result);
    }

    [InlineData("purpose1")]
    [InlineData("purpose1", "purpose2")]
    [InlineData("purpose1", "purpose2", "purpose3")]
    [Theory]
    public void UnprotectWithPurposes(params string[] purposes)
    {
        ArgumentNullException.ThrowIfNull(purposes);

        // Arrange
        var provider = BuildProvider(purposes);
        using var host = StartHost(provider);

        // Act
        var result = MachineKey.Unprotect(protectedData, purposes);

        // Assert
        Assert.Equal(plaintext, result);
    }

    private static IDataProtectionProvider BuildProvider(params string[] purposes)
    {
        const string DefaultPurpose = "User.MachineKey.Protect";

        var protectUnprotect = new Mock<IDataProtector>();
        protectUnprotect.Setup(m => m.Protect(plaintext)).Returns(protectedData);
        protectUnprotect.Setup(m => m.Unprotect(protectedData)).Returns(plaintext);

        var withPurposes = purposes.Reverse()
            .Aggregate(protectUnprotect.Object, (protector, purpose) =>
            {
                var mock = new Mock<IDataProtector>();
                mock.Setup(m => m.CreateProtector(purpose)).Returns(protector);
                return mock.Object;
            });

        var provider = new Mock<IDataProtectionProvider>();
        provider.Setup(p => p.CreateProtector(DefaultPurpose)).Returns(withPurposes);
        return provider.Object;
    }

    private static IDisposable StartHost(IDataProtectionProvider provider) => Host.CreateDefaultBuilder()
        .ConfigureWebHost(app =>
        {
            app.UseTestServer();
            app.Configure(app => { });
            app.ConfigureServices(services =>
            {
                services.AddSystemWebAdapters();
                services.AddSingleton(provider);
            });
        })
        .Start();
}
