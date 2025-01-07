// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class HostingEnvironmentTests
{
    [InlineData("/DownOneLevel/DownLevelPage.aspx", "DownOneLevel\\DownLevelPage.aspx")]
    [InlineData("~/MyUploadedFiles", "MyUploadedFiles")]
    [InlineData("/UploadedFiles", "UploadedFiles")]
    [InlineData("/NotRealFolder", "NotRealFolder")]
    [InlineData("/TrailingSlash/", "TrailingSlash\\")]
    [InlineData("\\TrailingSlash2\\", "TrailingSlash2\\")]
    [InlineData("\\\\SomeServer\\Share\\Path", "SomeServer\\Share\\Path")]
    [InlineData("//SomeServer/Share/Path", "SomeServer\\Share\\Path")]
    [InlineData("/", "\\")]
    [InlineData("~/", "\\")]
    [Theory]
    public void MapPath(string virtualPath, string expectedRelativePath)
    {
        // Arrange
        var options = new SystemWebAdaptersOptions
        {
            AppDomainAppVirtualPath = "/",
            AppDomainAppPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\ExampleSites\TestMapPath" : "/apps/test-map-path"
        };
        var ioptions = Options.Create(options);
        var utility = new MapPathUtility(ioptions, new(ioptions));

        var services = new ServiceCollection();
        services.AddSingleton<IMapPathUtility>(utility);
        var serviceProvider = services.BuildServiceProvider();

        string result;

        // using ensures Dispose() runs which clears the static "Current" property.
        using (var hostingEnvironmentAccessor = new HostingEnvironmentAccessor(serviceProvider, ioptions))
        {
            // Act
            result = HostingEnvironment.MapPath(virtualPath);
        }

        var expected = System.IO.Path.Join(options.AppDomainAppPath, expectedRelativePath);

        // Assert
        Assert.Equal(expected, result);
    }

    [InlineData("../OutsideApplication", typeof(ArgumentException))]
    [InlineData("C:\\OutsideApplication", typeof(ArgumentException))]
    [InlineData("../RootLevelPage.aspx", typeof(ArgumentException))]
    [InlineData("/../OutsideApplication.aspx", typeof(HttpException))]
    [InlineData("File", typeof(ArgumentException))]
    [InlineData("", typeof(ArgumentNullException))]
    [InlineData(null, typeof(ArgumentNullException))]
    [Theory]
    public void MapPathException(string? virtualPath, Type expected)
    {
        // Arrange
        var options = new SystemWebAdaptersOptions
        {
            AppDomainAppVirtualPath = "/",
            AppDomainAppPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\ExampleSites\TestMapPath" : "/apps/test-map-path"
        };
        var ioptions = Options.Create(options);
        var utility = new MapPathUtility(ioptions, new(ioptions));

        var services = new ServiceCollection();
        services.AddSingleton<IMapPathUtility>(utility);
        var serviceProvider = services.BuildServiceProvider();

        // using ensures Dispose() runs which clears the static "Current" property.
        using (var hostingEnvironmentAccessor = new HostingEnvironmentAccessor(serviceProvider, ioptions))
        {
            // Assert
            Assert.Throws(expected, () => HostingEnvironment.MapPath(virtualPath));
        }
    }
}
