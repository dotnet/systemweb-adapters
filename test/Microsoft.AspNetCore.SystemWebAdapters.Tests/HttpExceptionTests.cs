// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Web;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "Used for tests")]
public class HttpExceptionTests
{
    [Fact]
    public void DefaultValues()
    {
        // Act
        var exception = new HttpException();

        // Assert
        Assert.Equal(500, exception.GetHttpCode());
    }

    [Fact]
    public void Create()
    {
        // Arrange
        var message = "message";

        // Act
        var exception = new HttpException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(500, exception.GetHttpCode());
    }

    [Fact]
    public void CreateWithInnerException()
    {
        // Arrange
        var message = "message";
        var innerException = new Exception();

        // Act
        var exception = new HttpException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
        Assert.Equal(500, exception.GetHttpCode());
    }

    [Fact]
    public void CreateWithHttpStatusCode()
    {
        // Arrange
        var httpStatusCode = 404;

        // Act
        var exception = new HttpException(httpStatusCode, "message");

        // Assert
        Assert.Equal(httpStatusCode, exception.GetHttpCode());
    }

    [Fact]
    public void CreateWithHttpStatusCodeAndMessage()
    {
        // Arrange
        var httpStatusCode = 404;
        var message = "message";

        // Act
        var exception = new HttpException(httpStatusCode, message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(httpStatusCode, exception.GetHttpCode());
    }

    [Fact]
    public void CreateWithIntCodeAndMessageAndInnerException()
    {
        // Arrange
        var httpStatusCode = 404;
        var message = "message";
        var innerException = new Exception();

        // Act
        var exception = new HttpException(httpStatusCode, message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(httpStatusCode, exception.GetHttpCode());
    }

    [Fact]
    public void PathTooLongCode()
    {
        // Arrange
        var exception = new HttpException(string.Empty, new PathTooLongException());

        // Act
        var httpStatusCode = exception.GetHttpCode();

        // Assert
        Assert.Equal(StatusCodes.Status414UriTooLong, httpStatusCode);
    }

    [Fact]
    public void UnauthorizedCode()
    {
        // Arrange
        var exception = new HttpException(string.Empty, new UnauthorizedAccessException());

        // Act
        var httpStatusCode = exception.GetHttpCode();

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, httpStatusCode);
    }
}
