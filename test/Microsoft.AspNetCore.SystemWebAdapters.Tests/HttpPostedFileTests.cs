// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System;
using System.IO;
using System.Web;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class HttpPostedFileTests
{
    private readonly Fixture _fixture;

    public HttpPostedFileTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void InternalFile()
    {
        // Arrange
        var formFile = new Mock<IFormFile>();
        var posted = new HttpPostedFile(formFile.Object);

        // Act
        var file = posted.File;

        // Assert
        Assert.Same(formFile.Object, file);
    }

    [Fact]
    public void FileName()
    {
        // Arrange
        var expected = _fixture.Create<string>();
        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns(expected);

        var posted = new HttpPostedFile(file.Object);

        // Act
        var fileName = posted.FileName;

        // Assert
        Assert.Equal(expected, fileName);
    }

    [Fact]
    public void ContentType()
    {
        // Arrange
        var expected = _fixture.Create<string>();
        var file = new Mock<IFormFile>();
        file.Setup(f => f.ContentType).Returns(expected);

        var posted = new HttpPostedFile(file.Object);

        // Act
        var contentType = posted.ContentType;

        // Assert
        Assert.Equal(expected, contentType);
    }

    [Fact]
    public void ContentLength()
    {
        // Arrange
        var expected = _fixture.Create<int>();
        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(expected);

        var posted = new HttpPostedFile(file.Object);

        // Act
        var length = posted.ContentLength;

        // Assert
        Assert.Equal(expected, length);
    }

    [Fact]
    public void InputStream()
    {
        // Arrange
        var expected = new Mock<Stream>();
        var file = new Mock<IFormFile>();
        file.Setup(f => f.OpenReadStream()).Returns(expected.Object);

        var posted = new HttpPostedFile(file.Object);

        // Act
        var stream = posted.InputStream;

        // Assert
        Assert.Equal(expected.Object, stream);
    }

    [Fact]
    public void SaveAsForInvalidRootPath()
    {
        // Arrange
        var file = new Mock<IFormFile>();
        var expectedStream = new Mock<Stream>();
        file.Setup(f => f.OpenReadStream()).Returns(expectedStream.Object);
        var posted = new HttpPostedFile(file.Object);

        //Act and Assert
        Assert.Throws<HttpException>(() => posted.SaveAs("InvalidPath"));
    }

    [Fact]
    public void SaveAsWithValidRootPath()
    {
        // Arrange
        var file = new Mock<IFormFile>();
        var expectedStream = new Mock<Stream>();
        file.Setup(f => f.OpenReadStream()).Returns(expectedStream.Object);
        string validTempPath = string.Format(CultureInfo.InvariantCulture, @"{0}{1}.txt", Path.GetTempPath(), Guid.NewGuid());
        var posted = new HttpPostedFile(file.Object);

        //Act
        posted.SaveAs(validTempPath);

        //Assert
        Assert.True(File.Exists(validTempPath), "Temp file should be created");

        //Cleanup
        File.Delete(validTempPath);
    }
}
