// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class HttpRuntimeIntegrationTests : SelfHostedTestBase
{
    private const string IIS_VERSION = "IIS_Version";
    private const string IIS_SITE_ID = "IIS_SITE_ID";
    private const string IIS_SITE_NAME = "IIS_SITE_NAME";
    private const string IIS_APPLICATION_VIRTUAL_PATH = "IIS_APPLICATION_VIRTUAL_PATH";
    private const string IIS_PHYSICAL_PATH = "IIS_PHYSICAL_PATH";
    private const string IIS_APPLICATION_ID = "IIS_APPLICATION_ID";
    private const string IIS_APP_CONFIG_PATH = "IIS_APP_CONFIG_PATH";
    private const string IIS_APP_POOL_CONFIG_FILE = "IIS_APP_POOL_CONFIG_FILE";
    private const string IIS_APP_POOL_ID = "IIS_APP_POOL_ID";

    [Fact]
    public async Task ConfigureRuntimeViaConfig()
    {
        // Arrange
        using var host = await GetTestHost()
            .ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    [IIS_VERSION] = "10.0",
                    [IIS_SITE_ID] = "1",
                    [IIS_APP_POOL_ID] = IIS_APP_POOL_ID,
                    [IIS_APP_POOL_CONFIG_FILE] = IIS_APP_POOL_CONFIG_FILE,
                    [IIS_APP_CONFIG_PATH] = IIS_APP_CONFIG_PATH,
                    [IIS_PHYSICAL_PATH] = IIS_PHYSICAL_PATH,
                    [IIS_APPLICATION_VIRTUAL_PATH] = IIS_APPLICATION_VIRTUAL_PATH,
                    [IIS_APPLICATION_ID] = IIS_APPLICATION_ID,
                    [IIS_SITE_NAME] = IIS_SITE_NAME,
                });
            })
            .StartAsync();

        // Act
        var options = host.Services.GetRequiredService<IOptions<SystemWebAdaptersOptions>>().Value;

        // Assert
        Assert.Equal(IIS_SITE_NAME, options.SiteName);
        Assert.Equal(IIS_APPLICATION_VIRTUAL_PATH, options.AppDomainAppVirtualPath);
        Assert.Equal(IIS_PHYSICAL_PATH, options.AppDomainAppPath);
        Assert.Equal(IIS_APPLICATION_ID, options.ApplicationID);
        Assert.True(options.IsHosted);
    }

    [Fact]
    public async Task ConfigureRuntimeViaFeature()
    {
        // Arrange
        var feature = new Mock<IIISEnvironmentFeature>();

        feature.Setup(f => f.SiteName).Returns(IIS_SITE_NAME);
        feature.Setup(f => f.ApplicationVirtualPath).Returns(IIS_APPLICATION_VIRTUAL_PATH);
        feature.Setup(f => f.ApplicationPhysicalPath).Returns(IIS_PHYSICAL_PATH);
        feature.Setup(f => f.ApplicationId).Returns(IIS_APPLICATION_ID);
        feature.Setup(f => f.AppConfigPath).Returns(IIS_APP_CONFIG_PATH);
        feature.Setup(f => f.AppPoolConfigFile).Returns(IIS_APP_POOL_CONFIG_FILE);
        feature.Setup(f => f.AppPoolId).Returns(IIS_APP_POOL_ID);

        using var host = await GetTestHost()
            .StartAsync();

        host.GetTestServer().Features.Set<IIISEnvironmentFeature>(feature.Object);

        // Act
        var options = host.Services.GetRequiredService<IOptions<SystemWebAdaptersOptions>>().Value;

        // Assert
        Assert.Equal(IIS_SITE_NAME, options.SiteName);
        Assert.Equal(IIS_APPLICATION_VIRTUAL_PATH, options.AppDomainAppVirtualPath);
        Assert.Equal(IIS_PHYSICAL_PATH, options.AppDomainAppPath);
        Assert.Equal(IIS_APPLICATION_ID, options.ApplicationID);
        Assert.True(options.IsHosted);
    }
}
