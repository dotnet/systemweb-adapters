// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class HttpCacheVaryByContentEncodingsTests
{
    [Fact]
    public void ConstructorSetsDefaultState()
    {
        // Arrange & Act
        var encodings = new HttpCacheVaryByContentEncodings();
        
        // Assert
        Assert.Null(encodings.GetContentEncodings());
        Assert.False(encodings.IsModified());
    }
    
    [Fact]
    public void ResetClearsAllState()
    {
        // Arrange
        var encodings = new HttpCacheVaryByContentEncodings();
        encodings["gzip"] = true;
        Assert.NotNull(encodings.GetContentEncodings());
        Assert.True(encodings.IsModified());
        
        // Act
        encodings.Reset();
        
        // Assert
        Assert.Null(encodings.GetContentEncodings());
        Assert.False(encodings.IsModified());
    }
    
    [Fact]
    public void SetContentEncodingsNullResetsState()
    {
        // Arrange
        var encodings = new HttpCacheVaryByContentEncodings();
        encodings["gzip"] = true;
        
        // Act
        encodings.SetContentEncodings(null);
        
        // Assert
        Assert.Null(encodings.GetContentEncodings());
        Assert.False(encodings.IsModified());
    }
    
    [Fact]
    public void SetContentEncodingsWithValuesSetsEncodingsAndIsModified()
    {
        // Arrange
        var encodings = new HttpCacheVaryByContentEncodings();
        var contentEncodings = new[] { "gzip", "deflate" };
        
        // Act
        encodings.SetContentEncodings(contentEncodings);
        
        // Assert
        Assert.Equal(contentEncodings, encodings.GetContentEncodings());
        Assert.True(encodings.IsModified());
    }
    
    [Fact]
    public void GetContentEncodingsReturnsDeepCopy()
    {
        // Arrange
        var encodings = new HttpCacheVaryByContentEncodings();
        var contentEncodings = new[] { "gzip", "deflate" };
        encodings.SetContentEncodings(contentEncodings);
        
        // Act
        var result = encodings.GetContentEncodings();
        result![0] = "changed"; // Modify the returned array
        
        // Assert
        // Original array should not be changed
        Assert.Equal("gzip", encodings.GetContentEncodings()![0]);
    }
    
    [Fact]
    public void IndexerThrowsIfNullOrWhitespaceContentEncoding()
    {
        // Arrange
        var encodings = new HttpCacheVaryByContentEncodings();
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => encodings[null!] = true);
        Assert.Throws<ArgumentNullException>(() => _ = encodings[null!]);
        Assert.Throws<ArgumentException>(() => encodings[""] = true);
        Assert.Throws<ArgumentException>(() => _ = encodings["  "]);
    }
    
    [Fact]
    public void IndexerGetReturnsFalseByDefault()
    {
        // Arrange
        var encodings = new HttpCacheVaryByContentEncodings();
        
        // Act & Assert
        Assert.False(encodings["gzip"]);
    }
    
    [Fact]
    public void IndexerSetTrueAddsEncoding()
    {
        // Arrange
        var encodings = new HttpCacheVaryByContentEncodings();
        
        // Act
        encodings["gzip"] = true;
        
        // Assert
        Assert.True(encodings["gzip"]);
        Assert.Equal(new[] { "gzip" }, encodings.GetContentEncodings());
        Assert.True(encodings.IsModified());
    }
    
    [Fact]
    public void IndexerSetFalseDoesNothing()
    {
        // Arrange
        var encodings = new HttpCacheVaryByContentEncodings();
        encodings["gzip"] = true;
        
        // Act
        encodings["gzip"] = false;
        encodings["deflate"] = false;
        
        // Assert
        Assert.True(encodings["gzip"]); // Still true
        Assert.False(encodings["deflate"]); // Still false
        Assert.Equal(new[] { "gzip" }, encodings.GetContentEncodings());
    }
    
    [Fact]
    public void IndexerSetMultipleEncodingsAppendsThem()
    {
        // Arrange
        var encodings = new HttpCacheVaryByContentEncodings();
        
        // Act
        encodings["gzip"] = true;
        encodings["deflate"] = true;
        encodings["br"] = true;
        
        // Assert
        Assert.True(encodings["gzip"]);
        Assert.True(encodings["deflate"]);
        Assert.True(encodings["br"]);
        Assert.Contains("gzip", encodings.GetContentEncodings()!);
        Assert.Contains("deflate", encodings.GetContentEncodings()!);
        Assert.Contains("br", encodings.GetContentEncodings()!);
        Assert.Equal(3, encodings.GetContentEncodings()!.Length);
    }

    [Theory]
    [InlineData(null, true)]  // No Content-Encoding header is cacheable
    [InlineData("unknown", false)]  // Unknown encoding is not cacheable
    [InlineData("gzip", true)]  // Listed encoding is cacheable
    public void IsCacheableEncodingChecksContentEncodings(string? contentEncoding, bool expected)
    {
        // Arrange
        var encodings = new HttpCacheVaryByContentEncodings();
        encodings["gzip"] = true;
        
        // Act
        var result = encodings.IsCacheableEncoding(contentEncoding!);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void IsCacheableEncodingAlwaysTrueWhenNoEncodingsSpecified()
    {
        // Arrange
        var encodings = new HttpCacheVaryByContentEncodings();
        
        // Act & Assert
        Assert.True(encodings.IsCacheableEncoding("anything"));
        Assert.True(encodings.IsCacheableEncoding(null!));
    }

    [Fact]
    public void IsModifiedIsFalseByDefault()
    {
        // Arrange & Act
        var encodings = new HttpCacheVaryByContentEncodings();
        
        // Assert
        Assert.False(encodings.IsModified());
    }

    [Fact]
    public void IsModifiedIsTrueAfterSettingEncoding()
    {
        // Arrange
        var encodings = new HttpCacheVaryByContentEncodings();
        
        // Act
        encodings["gzip"] = true;
        
        // Assert
        Assert.True(encodings.IsModified());
    }

    [Fact]
    public void IsModifiedIsTrueAfterSetContentEncodings()
    {
        // Arrange
        var encodings = new HttpCacheVaryByContentEncodings();
        
        // Act
        encodings.SetContentEncodings(new[] { "gzip" });
        
        // Assert
        Assert.True(encodings.IsModified());
    }

    [Fact]
    public void IsModifiedIsFalseAfterReset()
    {
        // Arrange
        var encodings = new HttpCacheVaryByContentEncodings();
        encodings["gzip"] = true;
        Assert.True(encodings.IsModified());
        
        // Act
        encodings.Reset();
        
        // Assert
        Assert.False(encodings.IsModified());
    }
}
