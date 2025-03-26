// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class HttpCachePolicyTests
{
    private static readonly DateTime DefaultTimestamp = new DateTime(2023, 6, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime SlidingTimestamp = new DateTime(2023, 6, 15, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ConstructorInitializesDefaultState()
    {
        // Arrange & Act
        var policy = new HttpCachePolicy();

        // Assert
        Assert.False(policy.IsModified());
        Assert.False(policy.GetNoServerCaching());
        Assert.False(policy.GetNoStore());
        Assert.False(policy.GetNoTransforms());
        Assert.False(policy.GetIgnoreRangeRequests());
        Assert.Null(policy.GetVaryByCustom());
        Assert.Null(policy.GetCacheExtensions());
        Assert.Equal((HttpCacheability)6, policy.GetCacheability());
        Assert.Equal(DateTime.MinValue, policy.GetExpires());
        Assert.Equal(TimeSpan.Zero, policy.GetMaxAge());
        Assert.Equal(TimeSpan.Zero, policy.GetProxyMaxAge());
        Assert.False(policy.HasSlidingExpiration());
        Assert.False(policy.IsValidUntilExpires());
        Assert.Equal(HttpCacheRevalidation.None, policy.GetRevalidation());
        Assert.Null(policy.GetETag());
        Assert.Equal(DateTime.MinValue, policy.GetUtcLastModified());
        Assert.False(policy.GetLastModifiedFromFileDependencies());
        Assert.False(policy.GetETagFromFileDependencies());
        Assert.Equal(-1, policy.GetOmitVaryStar());
    }

    [Fact]
    public void VaryByPropertiesReturnNonNullInstances()
    {
        // Arrange & Act
        var policy = new HttpCachePolicy();

        // Assert
        Assert.NotNull(policy.VaryByContentEncodings);
        Assert.NotNull(policy.VaryByHeaders);
        Assert.NotNull(policy.VaryByParams);
    }

    [Fact]
    public void IsModifiedReturnsFalseByDefault()
    {
        // Arrange & Act
        var policy = new HttpCachePolicy();

        // Assert
        Assert.False(policy.IsModified());
    }

    [Fact]
    public void IsModifiedReturnsTrueAfterModification()
    {
        // Arrange
        var policy = new HttpCachePolicy();

        // Act
        policy.SetNoStore();

        // Assert
        Assert.True(policy.IsModified());
    }

    [Fact]
    public void IsModifiedReturnsTrueWhenVaryByIsModified()
    {
        // Arrange
        var policy = new HttpCachePolicy();

        // Act
        policy.VaryByHeaders.UserAgent = true;

        // Assert
        Assert.True(policy.IsModified());
    }

    [Theory]
    [InlineData(HttpCacheability.NoCache)]
    [InlineData(HttpCacheability.Private)]
    [InlineData(HttpCacheability.ServerAndNoCache)]
    [InlineData(HttpCacheability.Public)]
    [InlineData(HttpCacheability.ServerAndPrivate)]
    public void SetCacheabilityUpdatesValueCorrectly(HttpCacheability cacheability)
    {
        // Arrange
        var policy = new HttpCachePolicy();

        // Act
        policy.SetCacheability(cacheability);

        // Assert
        Assert.Equal(cacheability, policy.GetCacheability());
        Assert.True(policy.IsModified());
    }

    [Fact]
    public void SetCacheabilityThrowsExceptionForInvalidEnum()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        var invalidCacheability = (HttpCacheability)999;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => policy.SetCacheability(invalidCacheability));
    }

    [Theory]
    [InlineData(HttpCacheability.Private)]
    [InlineData(HttpCacheability.NoCache)]
    public void SetCacheabilityWithFieldAddsFieldToCollection(HttpCacheability cacheability)
    {
        // Arrange
        var policy = new HttpCachePolicy();
        var field = "Content-Type";

        // Act
        policy.SetCacheability(cacheability, field);

        // Assert
        Assert.True(policy.IsModified());
    }

    [Fact]
    public void SetCacheabilityWithFieldThrowsForInvalidCacheability()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        var field = "Content-Type";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => policy.SetCacheability(HttpCacheability.Public, field));
    }

    [Fact]
    public void SetCacheabilityWithFieldThrowsForNullField()
    {
        // Arrange
        var policy = new HttpCachePolicy();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => policy.SetCacheability(HttpCacheability.Private, null!));
    }

    [Fact]
    public void SetNoStoreSetsNoStoreFlag()
    {
        // Arrange
        var policy = new HttpCachePolicy();

        // Act
        policy.SetNoStore();

        // Assert
        Assert.True(policy.GetNoStore());
        Assert.True(policy.IsModified());
    }

    [Fact]
    public void SetNoServerCachingSetsNoServerCachingFlag()
    {
        // Arrange
        var policy = new HttpCachePolicy();

        // Act
        policy.SetNoServerCaching();

        // Assert
        Assert.True(policy.GetNoServerCaching());
        Assert.True(policy.IsModified());
    }

    [Fact]
    public void SetNoTransformsSetsNoTransformsFlag()
    {
        // Arrange
        var policy = new HttpCachePolicy();

        // Act
        policy.SetNoTransforms();

        // Assert
        Assert.True(policy.GetNoTransforms());
        Assert.True(policy.IsModified());
    }

    [Fact]
    public void AppendCacheExtensionAddsExtension()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        var extension = "immutable";

        // Act
        policy.AppendCacheExtension(extension);

        // Assert
        Assert.Equal(extension, policy.GetCacheExtensions());
        Assert.True(policy.IsModified());
    }

    [Fact]
    public void AppendCacheExtensionAppendsMultipleExtensions()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        var extension1 = "immutable";
        var extension2 = "stale-if-error=60";

        // Act
        policy.AppendCacheExtension(extension1);
        policy.AppendCacheExtension(extension2);

        // Assert
        Assert.Equal($"{extension1}, {extension2}", policy.GetCacheExtensions());
    }

    [Fact]
    public void AppendCacheExtensionThrowsForNullExtension()
    {
        // Arrange
        var policy = new HttpCachePolicy();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => policy.AppendCacheExtension(null!));
    }

    [Fact]
    public void SetVaryByCustomSetsCustomValue()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        var customValue = "browser";

        // Act
        policy.SetVaryByCustom(customValue);

        // Assert
        Assert.Equal(customValue, policy.GetVaryByCustom());
        Assert.True(policy.IsModified());
    }

    [Fact]
    public void SetVaryByCustomThrowsWhenCalledMultipleTimes()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetVaryByCustom("browser");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => policy.SetVaryByCustom("user-agent"));
    }

    [Fact]
    public void SetVaryByCustomThrowsForNullValue()
    {
        // Arrange
        var policy = new HttpCachePolicy();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => policy.SetVaryByCustom(null!));
    }

    [Theory]
    [InlineData(HttpCacheRevalidation.AllCaches)]
    [InlineData(HttpCacheRevalidation.ProxyCaches)]
    [InlineData(HttpCacheRevalidation.None)]
    public void SetRevalidationUpdatesValueCorrectly(HttpCacheRevalidation revalidation)
    {
        // Arrange
        var policy = new HttpCachePolicy();

        // Act)
        policy.SetRevalidation(revalidation);

        // Assert
        Assert.Equal(revalidation, policy.GetRevalidation());

        if (revalidation == HttpCacheRevalidation.None)
        {
            Assert.False(policy.IsModified());
        }
        else
        {
            Assert.True(policy.IsModified());
        }
    }

    [Fact]
    public void SetRevalidationThrowsExceptionForInvalidEnum()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        var invalidRevalidation = (HttpCacheRevalidation)999;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => policy.SetRevalidation(invalidRevalidation));
    }

    [Fact]
    public void SetETagSetsETagValue()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        var etag = "\"abc123\"";

        // Act
        policy.SetETag(etag);

        // Assert
        Assert.Equal(etag, policy.GetETag());
        Assert.True(policy.IsModified());
    }

    [Fact]
    public void SetETagThrowsWhenCalledMultipleTimes()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetETag("\"abc123\"");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => policy.SetETag("\"xyz789\""));
    }

    [Fact]
    public void SetETagThrowsWhenETagFromFileDependenciesIsSet()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetETagFromFileDependencies();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => policy.SetETag("\"abc123\""));
    }

    [Fact]
    public void SetETagFromFileDependenciesSetsETagFileGenerationFlag()
    {
        // Arrange
        var policy = new HttpCachePolicy();

        // Act
        policy.SetETagFromFileDependencies();

        // Assert
        Assert.True(policy.GetETagFromFileDependencies());
        Assert.True(policy.IsModified());
    }

    [Fact]
    public void SetETagFromFileDependenciesThrowsWhenETagIsAlreadySet()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetETag("\"abc123\"");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => policy.SetETagFromFileDependencies());
    }

    [Fact]
    public void SetLastModifiedSetsLastModifiedDateCorrectly()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        var date = new DateTime(2023, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        policy.SetLastModified(date);

        // Assert
        // The date should be truncated to seconds
        var expectedDate = new DateTime(date.Ticks - (date.Ticks % TimeSpan.TicksPerSecond));
        Assert.Equal(expectedDate, policy.GetUtcLastModified());
        Assert.True(policy.IsModified());
    }

    [Fact]
    public void SetLastModifiedFromFileDependenciesSetsFileGenerationFlag()
    {
        // Arrange
        var policy = new HttpCachePolicy();

        // Act
        policy.SetLastModifiedFromFileDependencies();

        // Assert
        Assert.True(policy.GetLastModifiedFromFileDependencies());
        Assert.True(policy.IsModified());
    }

    [Fact]
    public void SetOmitVaryStarUpdatesOmitVaryStarFlag()
    {
        // Arrange
        var policy = new HttpCachePolicy();

        // Act
        policy.SetOmitVaryStar(true);

        // Assert
        Assert.Equal(1, policy.GetOmitVaryStar());
        Assert.True(policy.IsModified());
    }

    [Fact]
    public void SetSlidingExpirationUpdatesSlidingExpirationFlag()
    {
        // Arrange
        var policy = new HttpCachePolicy();

        // Act
        policy.SetSlidingExpiration(true);

        // Assert
        Assert.True(policy.HasSlidingExpiration());
        Assert.True(policy.IsModified());
    }

    [Fact]
    public void SetValidUntilExpiresUpdatesValidUntilExpiresFlag()
    {
        // Arrange
        var policy = new HttpCachePolicy();

        // Act
        policy.SetValidUntilExpires(true);

        // Assert
        Assert.True(policy.IsValidUntilExpires());
        Assert.True(policy.IsModified());
    }

    [Fact]
    public void AddHeadersDefaultPolicyAddsPrivateHeader()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, DefaultTimestamp);

        // Assert
        Assert.Single(headers);
        Assert.Equal("private", headers[HeaderNames.CacheControl]);
    }

    [Fact]
    public void AddHeadersNoCacheAddsPragmaAndExpiresHeaders()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetCacheability(HttpCacheability.NoCache);
        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, DefaultTimestamp);

        // Assert
        Assert.Equal(3, headers.Count);
        Assert.Equal("no-cache", headers[HeaderNames.CacheControl]);
        Assert.Equal("no-cache", headers[HeaderNames.Pragma]);
        Assert.Equal("-1", headers[HeaderNames.Expires]);
    }

    [Fact]
    public void AddHeadersWithNoStoreAddsNoStoreHeader()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetNoStore();
        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, DefaultTimestamp);

        // Assert
        Assert.Single(headers);
        Assert.Equal("private, no-store", headers[HeaderNames.CacheControl]);
    }

    [Fact]
    public void AddHeadersWithNoTransformsAddsNoTransformHeader()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetNoTransforms();
        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, DefaultTimestamp);

        // Assert
        Assert.Single(headers);
        Assert.Equal("private, no-transform", headers[HeaderNames.CacheControl]);
    }

    [Fact]
    public void AddHeadersWithCacheExtensionAppendsCacheExtension()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.AppendCacheExtension("immutable");
        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, DefaultTimestamp);

        // Assert
        Assert.Single(headers);
        Assert.Equal("private, immutable", headers[HeaderNames.CacheControl]);
    }

    [Fact]
    public void AddHeadersWithRevalidationAddsRevalidationHeader()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetRevalidation(HttpCacheRevalidation.AllCaches);
        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, DefaultTimestamp);

        // Assert
        Assert.Single(headers);
        Assert.Equal("private, must-revalidate", headers[HeaderNames.CacheControl]);
    }

    [Fact]
    public void AddHeadersWithETagAddsETagHeader()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetCacheability(HttpCacheability.Public);
        policy.SetETag("\"abc123\"");
        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, DefaultTimestamp);

        // Assert
        Assert.Equal(2, headers.Count);
        Assert.Equal("public", headers[HeaderNames.CacheControl]);
        Assert.Equal("\"abc123\"", headers[HeaderNames.ETag]);
    }

    [Fact]
    public void AddHeadersWithLastModifiedAddsLastModifiedHeader()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        var lastModified = new DateTime(2023, 5, 1, 12, 0, 0, DateTimeKind.Utc);
        policy.SetLastModified(lastModified);
        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, DefaultTimestamp);

        // Assert
        Assert.Equal(2, headers.Count);
        Assert.Equal("private", headers[HeaderNames.CacheControl]);
        Assert.Equal("Mon, 01 May 2023 12:00:00 GMT", headers[HeaderNames.LastModified]);
    }

    [Fact]
    public void AddHeadersWithExpiresAddsExpiresHeader()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        var expires = new DateTime(2023, 7, 1, 12, 0, 0, DateTimeKind.Utc);
        policy.SetExpires(expires);
        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, DefaultTimestamp);

        // Assert
        Assert.Equal(2, headers.Count);
        Assert.Equal("private", headers[HeaderNames.CacheControl]);
        Assert.Equal("Sat, 01 Jul 2023 12:00:00 GMT", headers[HeaderNames.Expires]);
    }

    [Fact]
    public void AddHeadersWithSlidingExpirationCalculatesExpiresFromTimestamp()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetExpires(new DateTime(2023, 7, 1, 12, 0, 0, DateTimeKind.Utc));
        policy.SetSlidingExpiration(true);
        policy.UtcTimestampCreated = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc); // Set creation time

        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, SlidingTimestamp); // Use special sliding timestamp

        // Assert
        Assert.Equal(2, headers.Count);
        Assert.Equal("private", headers[HeaderNames.CacheControl]);
        Assert.Equal("Sat, 15 Jul 2023 12:00:00 GMT", headers[HeaderNames.Expires]);
    }

    [Fact]
    public void AddHeadersWithMaxAgeAddsMaxAgeHeader()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetMaxAge(TimeSpan.FromSeconds(3600));
        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, DefaultTimestamp);

        // Assert
        Assert.Single(headers);
        Assert.Equal("private, max-age=3600", headers[HeaderNames.CacheControl]);
    }

    [Fact]
    public void AddHeadersWithMaxAgeAndSlidingExpirationAddsMaxAgeHeader()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetMaxAge(TimeSpan.FromSeconds(3600));
        policy.SetSlidingExpiration(true);
        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, DefaultTimestamp);

        // Assert
        Assert.Single(headers);
        Assert.Equal("private, max-age=3600", headers[HeaderNames.CacheControl]);
    }

    [Fact]
    public void AddHeadersWithProxyMaxAgeAddsProxyMaxAgeHeader()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetProxyMaxAge(TimeSpan.FromSeconds(1800));
        policy.SetSlidingExpiration(true);
        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, DefaultTimestamp);

        // Assert
        Assert.Single(headers);
        Assert.Equal("private, s-maxage=1800", headers[HeaderNames.CacheControl]);
    }

    [Fact]
    public void AddHeadersWithVaryByHeadersAddsVaryHeader()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetCacheability(HttpCacheability.Public);
        policy.VaryByHeaders.UserAgent = true;
        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, DefaultTimestamp);

        // Assert
        Assert.Equal(2, headers.Count);
        Assert.Equal("public", headers[HeaderNames.CacheControl]);
        Assert.Equal("User-Agent", headers[HeaderNames.Vary]);
    }

    [Fact]
    public void AddHeadersWithVaryByCustomAddsVaryStarHeader()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetCacheability(HttpCacheability.Public);
        policy.SetVaryByCustom("browser");
        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, DefaultTimestamp);

        // Assert
        Assert.Equal(2, headers.Count);
        Assert.Equal("public", headers[HeaderNames.CacheControl]);
        Assert.Equal("*", headers[HeaderNames.Vary]);
    }

    [Fact]
    public void AddHeadersWithOmitVaryStarDoesNotAddVaryStarHeader()
    {
        // Arrange
        var policy = new HttpCachePolicy();
        policy.SetCacheability(HttpCacheability.Public);
        policy.SetVaryByCustom("browser");
        policy.SetOmitVaryStar(true);
        var headers = new HeaderDictionary();

        // Act
        policy.AddHeaders(headers, DefaultTimestamp);

        // Assert
        Assert.Single(headers);
        Assert.Equal("public", headers[HeaderNames.CacheControl]);
        // No Vary header should be present
    }
}
