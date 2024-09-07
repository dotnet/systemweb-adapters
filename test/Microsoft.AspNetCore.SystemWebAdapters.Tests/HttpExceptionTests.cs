// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net;
using System.Web;
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
        var exception = new HttpException(httpStatusCode);

        // Assert
        Assert.Equal(httpStatusCode, exception.GetHttpCode());
    }

    [Fact]
    public void CreateWithHttpStatusCodeEnum()
    {
        // Arrange
        var httpStatusCode = HttpStatusCode.NotFound;

        // Act
        var exception = new HttpException(httpStatusCode);

        // Assert
        Assert.Equal((int)httpStatusCode, exception.GetHttpCode());
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
    public void CreateWithHttpStatusCodeEnumAndMessage()
    {
        // Arrange
        var httpStatusCode = HttpStatusCode.NotFound;
        var message = "message";

        // Act
        var exception = new HttpException(httpStatusCode, message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal((int)httpStatusCode, exception.GetHttpCode());
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
    public void CreateWithEnumHttpStatusCodeAndMessageAndInnerException()
    {
        // Arrange
        var httpStatusCode = HttpStatusCode.NotFound;
        var message = "message";
        var innerException = new Exception();

        // Act
        var exception = new HttpException(httpStatusCode, message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal((int)httpStatusCode, exception.GetHttpCode());
    }
}
