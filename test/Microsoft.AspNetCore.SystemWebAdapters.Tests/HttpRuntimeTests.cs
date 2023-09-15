// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using AutoFixture;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class HttpRuntimeTests
{
    private readonly Fixture _fixture;

    public HttpRuntimeTests()
    {
        _fixture = new Fixture();

        // Note: Uncommenting the following lines fixes the issue
        //
        //var servicesCollection = new ServiceCollection();
        //servicesCollection.AddTransient<Cache>();

        //HostingEnvironmentAccessor.Current = new HostingEnvironmentAccessor(
        //    new DefaultServiceProviderFactory().CreateServiceProvider(servicesCollection),
        //    Options.Create(new SystemWebAdaptersOptions())
        //);
    }

    [Fact]
    public void SetCache()
    {
        // Arrange
        var cache = HttpRuntime.Cache;
        var cacheKey = _fixture.Create<string>();
        var cacheVal = _fixture.Create<string>();

        // Act
        cache[cacheKey] = cacheVal;

        // Assert
        Assert.Equal(cacheVal, cache[cacheKey]);
    }
}

