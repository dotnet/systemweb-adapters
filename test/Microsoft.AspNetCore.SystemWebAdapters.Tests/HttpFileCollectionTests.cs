// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Web;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class HttpFileCollectionTests
{
    private readonly Fixture _fixture;

    public HttpFileCollectionTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void AllKeysEmpty()
    {
        // Arrange
        var formFiles = new Mock<IFormFileCollection>();
        var collection = new HttpFileCollection(formFiles.Object);

        // Act
        var keys = collection.AllKeys;

        // Assert
        Assert.Empty(keys);
        Assert.Equal(Array.Empty<string>(), keys);
    }

    [Fact]
    public void KeysThrow()
    {
        // Arrange
        var formFiles = new Mock<IFormFileCollection>();
        var collection = new HttpFileCollection(formFiles.Object);

        // Act/Assert
        Assert.Throws<PlatformNotSupportedException>(() => collection.Keys);
    }

    [Fact]
    public void AllKeys()
    {
        // Arrange
        var expected = _fixture.CreateMany<string>().ToArray();
        var formFiles = new FormFileCollection();

        foreach (var name in expected)
        {
            var file = new Mock<IFormFile>();
            file.Setup(f => f.Name).Returns(name);
            formFiles.Add(file.Object);
        }

        var collection = new HttpFileCollection(formFiles);

        // Act
        var keys1 = collection.AllKeys;
        var keys2 = collection.AllKeys;

        // Assert
        Assert.Same(keys1, keys2);
        Assert.Equal(expected, keys1);
    }

    [Fact]
    public void GetEnumeratorTest()
    {
        // Arrange
        var expected = _fixture.CreateMany<string>().ToArray();
        var formFiles = new FormFileCollection();

        foreach (var name in expected)
        {
            var file = new Mock<IFormFile>();
            file.Setup(f => f.Name).Returns(name);
            formFiles.Add(file.Object);
        }

        var collection = new HttpFileCollection(formFiles);

        // Act
        var keys = collection.Cast<string>().ToArray(); // Calls .GetEnumerator()

        // Assert
        Assert.Equal(expected, keys);
    }

    [Fact]
    public void Count()
    {
        // Arrange
        var expected = _fixture.Create<int>();

        var formFiles = new Mock<IFormFileCollection>();
        formFiles.Setup(f => f.Count).Returns(expected);

        var collection = new HttpFileCollection(formFiles.Object);

        // Act
        var count = collection.Count;

        // Assert
        Assert.Equal(expected, count);
    }

    [Fact]
    public void GetNameDoesNotExist()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var formFiles = new Mock<IFormFileCollection>();
        var collection = new HttpFileCollection(formFiles.Object);

        // Act
        var result = collection.Get(name);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetName()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var formFile = new Mock<IFormFile>();

        var formFiles = new Mock<IFormFileCollection>();
        formFiles.Setup(f => f.GetFile(name)).Returns(formFile.Object);

        var collection = new HttpFileCollection(formFiles.Object);

        // Act
        var result = collection.Get(name);

        // Assert
        Assert.Same(formFile.Object, result!.File);
    }

    [Fact]
    public void GetIndex()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var formFile = new Mock<IFormFile>();

        var formFiles = new Mock<IFormFileCollection>();
        formFiles.Setup(f => f.GetFile(name)).Returns(formFile.Object);

        var collection = new HttpFileCollection(formFiles.Object);

        // Act
        var result = collection[name];

        // Assert
        Assert.Same(formFile.Object, result!.File);
    }

    [Fact]
    public void GetMultiple()
    {
        // Arrange
        var name1 = _fixture.Create<string>();
        var name2 = _fixture.Create<string>();

        var file1a = new Mock<IFormFile>();
        file1a.Setup(f => f.Name).Returns(name1);

        var file1b = new Mock<IFormFile>();
        file1b.Setup(f => f.Name).Returns(name1);

        var file2 = new Mock<IFormFile>();
        file2.Setup(f => f.Name).Returns(name2);

        var formFiles = new FormFileCollection
        {
            file1a.Object,
            file2.Object,
            file1b.Object,
        };

        var collection = new HttpFileCollection(formFiles);

        // Act
        var multiple = collection.GetMultiple(name1);

        // Assert
        Assert.True(multiple.IsReadOnly);
        Assert.Equal(2, multiple.Count);

        Assert.True(multiple.Contains(new(file1b.Object)));
        Assert.False(multiple.Contains(new(file2.Object)));

        Assert.Equal(0, multiple.IndexOf(new(file1a.Object)));
        Assert.Equal(1, multiple.IndexOf(new(file1b.Object)));
        Assert.Equal(-1, multiple.IndexOf(new(file2.Object)));

        Assert.Collection(multiple,
            f => Assert.Same(file1a.Object, f.File),
            f => Assert.Same(file1b.Object, f.File));

        // Use .GetEnumerator()
        Assert.Collection(multiple.OfType<HttpPostedFile>(),
            f => Assert.Same(file1a.Object, f.File),
            f => Assert.Same(file1b.Object, f.File));

        // Use .CopyTo(...)
        var array = new HttpPostedFile[2];
        multiple.CopyTo(array, 0);
        Assert.Collection(array,
            f => Assert.Same(file1a.Object, f.File),
            f => Assert.Same(file1b.Object, f.File));

        Assert.Same(file1b.Object, multiple[1].File);

        Assert.Throws<NotSupportedException>(() => multiple[0] = new HttpPostedFile(new Mock<IFormFile>().Object));
        Assert.Throws<NotSupportedException>(() => multiple.Add(new HttpPostedFile(new Mock<IFormFile>().Object)));
        Assert.Throws<NotSupportedException>(() => multiple.Clear());
        Assert.Throws<NotSupportedException>(() => multiple.Insert(1, new HttpPostedFile(new Mock<IFormFile>().Object)));
        Assert.Throws<NotSupportedException>(() => multiple.Remove(new HttpPostedFile(new Mock<IFormFile>().Object)));
        Assert.Throws<NotSupportedException>(() => multiple.RemoveAt(0));
    }
}
