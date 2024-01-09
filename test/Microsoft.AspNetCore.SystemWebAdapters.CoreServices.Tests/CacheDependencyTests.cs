// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using AutoFixture;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

/// <summary>
/// Tests for <see cref="CacheDependency"/>. These use <see cref="HttpRuntime.Cache"/> in some cases and must be tested within a host if cache keys are used
/// </summary>
[Collection(nameof(SelfHostedTests))]
public class CacheDependencyTests
{
    private readonly Fixture _fixture;

    public CacheDependencyTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void InsertWithDependency()
    {
        // Arrange
        var memCache = new Mock<MemoryCache>(_fixture.Create<string>(), null);
        var cache = new Cache(memCache.Object);
        var key = _fixture.Create<string>();
        var item = new object();
        var cacheDependency = new Mock<CacheDependency>();

        // Act
        cache.Insert(key, item, cacheDependency.Object);

        // Assert
        memCache.Verify(m => m.Set(key, item, It.Is<CacheItemPolicy>(e => e.AbsoluteExpiration.DateTime.Equals(Cache.NoAbsoluteExpiration) && e.SlidingExpiration.Equals(Cache.NoSlidingExpiration)), null), Times.Once);
    }

    [Fact]
    public async Task DependentFileCallback()
    {
        // Arrange
        using var memCache = new MemoryCache(_fixture.Create<string>());
        var cache = new Cache(memCache);
        var item = new object();
        var key = _fixture.Create<string>();
        var updated = false;
        var slidingExpiration = TimeSpan.FromMilliseconds(1);
        CacheItemUpdateReason? updateReason = default;

        var tcs = new TaskCompletionSource();

        void Callback(string key, CacheItemUpdateReason reason, out object? expensiveObject, out CacheDependency? dependency, out DateTime absoluteExpiration, out TimeSpan slidingExpiration)
        {
            expensiveObject = null;
            dependency = null;
            absoluteExpiration = Cache.NoAbsoluteExpiration;
            slidingExpiration = TimeSpan.FromMilliseconds(5);

            updated = true;
            updateReason = reason;

            tcs.SetResult();
        }

        var file = Path.GetTempFileName();
        await File.WriteAllTextAsync(file, key);

        using var cd = new CacheDependency(file);
        // Act
        cache.Insert(key, item, cd, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, Callback);

        // Ensure file is updated
        await File.WriteAllTextAsync(file, DateTime.UtcNow.ToString("O"));

        // Wait for callback to be called
        await tcs.Task;

        // Wait for callback to be finalized
        await Task.Delay(100);

        // Assert
        Assert.True(updated);
        Assert.Null(cache[key]);
        Assert.Equal(CacheItemUpdateReason.DependencyChanged, updateReason);
    }

    [Fact]
    public async Task DependentItemCallback()
    {
        // Arrange
        using var memCache = new MemoryCache(_fixture.Create<string>());
        var cache = new Cache(memCache);
        using var host = StartHost(cache);

        var item1 = new object();
        var item2 = new object();
        var key1 = _fixture.Create<string>();
        var key2 = _fixture.Create<string>();
        var updateReason = new Dictionary<string, CacheItemUpdateReason>();
        var slidingExpiration = TimeSpan.FromMilliseconds(1);

        void Callback(string key, CacheItemUpdateReason reason, out object? expensiveObject, out CacheDependency? dependency, out DateTime absoluteExpiration, out TimeSpan slidingExpiration)
        {
            expensiveObject = null;
            dependency = null;
            absoluteExpiration = Cache.NoAbsoluteExpiration;
            slidingExpiration = Cache.NoSlidingExpiration;

            updateReason[key] = reason;
        }

        // Act
        cache.Insert(key1, item1, null, Cache.NoAbsoluteExpiration, slidingExpiration, Callback);

        using var cd = new CacheDependency(null, new[] { key1 });
        cache.Insert(key2, item2, cd, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, Callback);

        Assert.Empty(updateReason);

        // Ensure sliding expiration has hit
        await Task.Delay(slidingExpiration);

        // Force cleanup to initiate callbacks on current thread
        memCache.Trim(100);

        // Assert
        Assert.Contains(key1, updateReason.Keys);
        Assert.Contains(key2, updateReason.Keys);

        Assert.Null(cache[key1]);
        Assert.Null(cache[key2]);

        Assert.Equal(CacheItemUpdateReason.Expired, updateReason[key1]);
        Assert.Equal(CacheItemUpdateReason.DependencyChanged, updateReason[key2]);
    }

    private static IDisposable StartHost(Cache cache) => Host.CreateDefaultBuilder()
        .ConfigureWebHost(app =>
        {
            app.UseTestServer();
            app.Configure(app => { });
            app.ConfigureServices(services =>
            {
                services.AddSystemWebAdapters();
                services.AddSingleton(cache);
            });
        })
        .Start();
}
