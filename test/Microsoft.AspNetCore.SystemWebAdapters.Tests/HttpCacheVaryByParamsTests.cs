using System;
using System.Web;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class HttpCacheVaryByParamsTests
{
    [Fact]
    public void SetParamsNullParametersResetsState()
    {
        // Arrange
        var varyByParams = new HttpCacheVaryByParams();

        // Act
        varyByParams.SetParams(null);

        // Assert
        Assert.False(varyByParams.IsModified());
        Assert.False(varyByParams.AcceptsParams());
    }

    [Fact]
    public void SetParamsEmptyStringSetsIgnoreParams()
    {
        // Arrange
        var varyByParams = new HttpCacheVaryByParams();

        // Act
        varyByParams.SetParams(new string[] { string.Empty });

        // Assert
        Assert.True(varyByParams.IsModified());
        Assert.True(varyByParams.IgnoreParams);
    }

    [Fact]
    public void SetParamsStarStringSetsParamsStar()
    {
        // Arrange
        var varyByParams = new HttpCacheVaryByParams();

        // Act
        varyByParams.SetParams(new string[] { "*" });

        // Assert
        Assert.True(varyByParams.IsModified());
        Assert.True(varyByParams.IsVaryByStar);
    }

    [Fact]
    public void SetParamsValidParametersSetsParameters()
    {
        // Arrange
        var varyByParams = new HttpCacheVaryByParams();
        var parameters = new string[] { "param1", "param2" };

        // Act
        varyByParams.SetParams(parameters);

        // Assert
        Assert.True(varyByParams.IsModified());
        Assert.True(varyByParams.AcceptsParams());
        Assert.Equal(parameters, varyByParams.GetParams());
    }

    [Fact]
    public void GetParamsNoParametersReturnsNull()
    {
        // Arrange
        var varyByParams = new HttpCacheVaryByParams();

        // Act
        var result = varyByParams.GetParams();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetParamsIgnoreParamsReturnsEmptyString()
    {
        // Arrange
        var varyByParams = new HttpCacheVaryByParams();
        varyByParams.IgnoreParams = true;

        // Act
        var result = varyByParams.GetParams();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(string.Empty, result[0]);
    }

    [Fact]
    public void GetParamsIsVaryByStarReturnsStar()
    {
        // Arrange
        var varyByParams = new HttpCacheVaryByParams();
        varyByParams.SetParams(new string[] { "*" });

        // Act
        var result = varyByParams.GetParams();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("*", result[0]);
    }

    [Theory]
    [InlineData("", true, false)]
    [InlineData("", false, false)]
    [InlineData("*", true, true)]
    [InlineData("*", false, false)]
    [InlineData("Content-Type", true, true)]
    [InlineData("Content-Type", false, false)]
    public void IndexerGetAndSetValues(string header, bool setValue, bool expected)
    {
        // Arrange
        var varyByParams = new HttpCacheVaryByParams();
        varyByParams[header] = setValue;

        // Act
        var result = varyByParams[HeaderNames.ContentType];

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IgnoreParamsSetAndGetValues()
    {
        // Arrange
        var varyByParams = new HttpCacheVaryByParams();

        // Act
        varyByParams.IgnoreParams = true;

        // Assert
        Assert.True(varyByParams.IgnoreParams);
    }


    [Fact]
    public void IgnoreParamsWhenStarSetHasNoEffect()
    {
        // Arrange
        var varyByParams = new HttpCacheVaryByParams();
        varyByParams["*"] = true;

        // Act
        varyByParams.IgnoreParams = true;

        // Assert
        Assert.False(varyByParams.IgnoreParams);
        Assert.True(varyByParams.IsVaryByStar);
    }

    [Fact]
    public void IndexerSetFalseHasNoEffect()
    {
        // Arrange
        var varyByParams = new HttpCacheVaryByParams();
        var paramName = "test";

        // Act
        varyByParams[paramName] = true;
        varyByParams[paramName] = false;

        // Assert
        Assert.True(varyByParams[paramName]);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void EmptyHeaderGetsIgnoreParamsIgnoreSet(bool value)
    {
        // Arrange
        var varyByParams = new HttpCacheVaryByParams();

        // Act
        varyByParams.IgnoreParams = value;
        var varyBy = varyByParams[string.Empty];

        // Assert
        Assert.Equal(value, varyBy);
    }

    [Fact]
    public void EmptyHeaderGetsIgnoreParamsIgnoreNotSet()
    {
        // Arrange
        var varyByParams = new HttpCacheVaryByParams();

        // Act
        var varyBy = varyByParams[string.Empty];

        // Assert
        Assert.False(varyBy);
    }

    [Fact]
    public void IndexerNullKeyThrowsArgumentNullException()
    {
        // Arrange
        var varyByParams = new HttpCacheVaryByParams();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => varyByParams[null!] = true);
        Assert.Throws<ArgumentNullException>(() => _ = varyByParams[null!]);
    }
}
