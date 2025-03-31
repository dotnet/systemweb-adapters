// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class HttpCacheVaryByHeadersTests
{
    [Fact]
    public void ConstructorSetsDefaultState()
    {
        // Arrange & Act
        var headers = new HttpCacheVaryByHeaders();

        // Assert
        Assert.False(headers.AcceptTypes);
        Assert.False(headers.UserLanguage);
        Assert.False(headers.UserAgent);
        Assert.False(headers.UserCharSet);
        Assert.Null(headers.GetHeaders());
        Assert.Null(headers.ToHeaderString());
        Assert.False(headers.GetVaryByUnspecifiedParameters());
    }

    [Fact]
    public void ResetClearsAllState()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();
        headers.UserAgent = true;
        headers.AcceptTypes = true;

        // Act
        headers.SetHeaders(null);

        // Assert
        Assert.False(headers.UserAgent);
        Assert.False(headers.AcceptTypes);
        Assert.Null(headers.GetHeaders());
    }

    [Fact]
    public void SetHeadersNullResetsState()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();
        headers.UserAgent = true;

        // Act
        headers.SetHeaders(null);

        // Assert
        Assert.Null(headers.GetHeaders());
        Assert.False(headers.GetVaryByUnspecifiedParameters());
    }

    [Fact]
    public void IndexerGetStarReturnsFalseByDefault()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();

        // Act & Assert
        Assert.False(headers["*"]);
        Assert.False(headers.GetVaryByUnspecifiedParameters());
    }

    [Fact]
    public void IndexerGetStarReturnsTrueAfterVaryByUnspecifiedParameters()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();
        headers.VaryByUnspecifiedParameters();

        // Act & Assert
        Assert.True(headers["*"]);
        Assert.True(headers.GetVaryByUnspecifiedParameters());
    }

    [Fact]
    public void IndexerSetStarTrueCallsVaryByUnspecifiedParameters()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();
        headers.AcceptTypes = true; // Add some header first

        // Act
        headers["*"] = true;

        // Assert
        Assert.True(headers["*"]);
        Assert.True(headers.GetVaryByUnspecifiedParameters());
        Assert.Equal(new[] { "*" }, headers.GetHeaders());
        Assert.Equal("*", headers.ToHeaderString());
        // Should clear other headers
        Assert.False(headers.AcceptTypes);
    }

    [Fact]
    public void IndexerSetStarFalseDoesNothing()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();
        headers.VaryByUnspecifiedParameters(); // Set _varyStar to true first

        // Act
        headers["*"] = false; // This should be ignored per the implementation

        // Assert
        Assert.True(headers["*"]); // Still true
        Assert.True(headers.GetVaryByUnspecifiedParameters()); // Still true
    }

    [Fact]
    public void RegularHeadersIgnoredWhenVaryStar()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();
        headers["*"] = true; // Set VaryByUnspecifiedParameters

        // Act
        headers.AcceptTypes = true; // Try to set a regular header
        headers.UserLanguage = true;
        headers["X-Custom-Header"] = true;

        // Assert
        Assert.True(headers.GetVaryByUnspecifiedParameters());
        Assert.Equal(new[] { "*" }, headers.GetHeaders());
        Assert.Equal("*", headers.ToHeaderString());
        // Headers should not be set when _varyStar is true
        Assert.False(headers.AcceptTypes);
        Assert.False(headers.UserLanguage);
    }

    [Fact]
    public void SetHeadersMultipleHeadersSetsCorrectly()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();
        var headerValues = new[] { "Accept", "User-Agent" };

        // Act
        headers.SetHeaders(headerValues);

        // Assert
        Assert.Equal(headerValues, headers.GetHeaders());
        Assert.False(headers.GetVaryByUnspecifiedParameters());
        Assert.Equal("Accept, User-Agent", headers.ToHeaderString());
    }

    [Theory]
    [InlineData("Accept")]
    [InlineData("Accept-Language")]
    [InlineData("User-Agent")]
    [InlineData("Accept-Charset")]
    public void BuiltInHeaderPropertiesSetAndGet(string headerName)
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();

        // Act & Assert
        headers[headerName] = true;
        Assert.True(headers[headerName]);
    }

    [Fact]
    public void CustomHeaderSetAndGet()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();
        var customHeader = "X-Custom-Header";

        // Act
        headers[customHeader] = true;

        // Assert
        Assert.True(headers[customHeader]);
    }

    [Fact]
    public void CustomHeaderSetFalseDoesNothing()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();
        var customHeader = "X-Custom-Header";

        // Act
        headers[customHeader] = true;
        headers[customHeader] = false;

        // Assert
        Assert.True(headers[customHeader]);
    }

    [Fact]
    public void VaryByUnspecifiedParametersSetsStarAndClearsHeaders()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();
        headers.AcceptTypes = true;
        headers.UserAgent = true;

        // Act
        headers.VaryByUnspecifiedParameters();

        // Assert
        Assert.True(headers.GetVaryByUnspecifiedParameters());
        Assert.Equal(new[] { "*" }, headers.GetHeaders());
        Assert.Equal("*", headers.ToHeaderString());
    }

    [Fact]
    public void IndexerNullHeaderThrowsArgumentNullException()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => headers[null!] = true);
        Assert.Throws<ArgumentNullException>(() => _ = headers[null!]);
    }

    [Fact]
    public void UserLanguageSetsCorrectHeader()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();

        // Act
        headers.UserLanguage = true;

        // Assert
        Assert.True(headers.UserLanguage);
        Assert.Contains("Accept-Language", headers.GetHeaders()!);
        Assert.Equal("Accept-Language", headers.ToHeaderString());
    }

    [Fact]
    public void UserCharSetSetsCorrectHeader()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();

        // Act
        headers.UserCharSet = true;

        // Assert
        Assert.True(headers.UserCharSet);
        Assert.Contains("Accept-Charset", headers.GetHeaders()!);
        Assert.Equal("Accept-Charset", headers.ToHeaderString());
    }

    [Fact]
    public void SetHeadersWithStarAsFirstHeaderSetsVaryStar()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();

        // Act
        headers.SetHeaders(new[] { "*" }); // Star as first, plus additional headers

        // Assert
        Assert.True(headers.GetVaryByUnspecifiedParameters());
        Assert.Equal(new[] { "*" }, headers.GetHeaders());
        Assert.Equal("*", headers.ToHeaderString());
    }

    [Fact]
    public void SetHeadersWithStarAsSecondHeaderTreatsStarAsNormalHeader()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();

        // Act
        headers.SetHeaders(new[] { "Accept", "*", "User-Agent" });

        // Assert
        Assert.False(headers.GetVaryByUnspecifiedParameters()); // Should not set VaryByUnspecifiedParameters
        Assert.Contains("*", headers.GetHeaders()!); // * should be treated as a regular header
        Assert.Contains("Accept", headers.GetHeaders()!);
        Assert.Contains("User-Agent", headers.GetHeaders()!);
        Assert.False(headers["*"]);
        Assert.True(headers["Accept"]);
        Assert.True(headers["User-Agent"]);
    }

    [Fact]
    public void IsModifiedIsFalseByDefault()
    {
        // Arrange & Act
        var headers = new HttpCacheVaryByHeaders();

        // Assert
        Assert.False(headers.IsModified());
    }

    [Fact]
    public void IsModifiedIsTrueAfterSettingAcceptTypes()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();

        // Act
        headers.AcceptTypes = true;

        // Assert
        Assert.True(headers.IsModified());
    }

    [Fact]
    public void IsModifiedIsTrueAfterSettingUserLanguage()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();

        // Act
        headers.UserLanguage = true;

        // Assert
        Assert.True(headers.IsModified());
    }

    [Fact]
    public void IsModifiedIsTrueAfterSettingUserAgent()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();

        // Act
        headers.UserAgent = true;

        // Assert
        Assert.True(headers.IsModified());
    }

    [Fact]
    public void IsModifiedIsTrueAfterSettingUserCharSet()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();

        // Act
        headers.UserCharSet = true;

        // Assert
        Assert.True(headers.IsModified());
    }

    [Fact]
    public void IsModifiedIsTrueAfterSettingCustomHeader()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();

        // Act
        headers["X-Custom-Header"] = true;

        // Assert
        Assert.True(headers.IsModified());
    }

    [Fact]
    public void IsModifiedIsTrueAfterSettingStarHeader()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();

        // Act
        headers["*"] = true;

        // Assert
        Assert.True(headers.IsModified());
    }

    [Fact]
    public void IsModifiedIsTrueAfterVaryByUnspecifiedParameters()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();

        // Act
        headers.VaryByUnspecifiedParameters();

        // Assert
        Assert.True(headers.IsModified());
    }

    [Fact]
    public void IsModifiedIsFalseAfterSetHeadersWithNull()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();
        headers.AcceptTypes = true; // Set IsModified to true
        Assert.True(headers.IsModified()); // Verify initial state

        // Act
        headers.SetHeaders(null);

        // Assert
        Assert.False(headers.IsModified());
    }

    [Fact]
    public void IsModifiedIsTrueAfterSetHeadersWithValues()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();

        // Act
        headers.SetHeaders(new[] { "Accept", "User-Agent" });

        // Assert
        Assert.True(headers.IsModified());
    }

    [Fact]
    public void IsModifiedRemainsUnchangedWhenSettingHeaderToFalse()
    {
        // Arrange
        var headers = new HttpCacheVaryByHeaders();
        headers.AcceptTypes = true;
        Assert.True(headers.IsModified());

        // Act
        headers["Accept"] = false; // This should be ignored

        // Assert
        Assert.True(headers.IsModified()); // Still true
        Assert.True(headers["Accept"]); // Header still set
    }
}
