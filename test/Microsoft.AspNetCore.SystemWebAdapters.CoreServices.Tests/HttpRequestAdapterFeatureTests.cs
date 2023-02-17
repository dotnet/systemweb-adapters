// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class HttpRequestAdapterFeatureTests
{
    [Fact]
    public void InputStream()
    {
        // Arrange
        var stream = new Mock<Stream>();

        var other = new Mock<IHttpRequestFeature>();
        other.Setup(o => o.Body).Returns(stream.Object);

        using var feature = new HttpRequestInputStreamFeature(other.Object);
        var adapterFeature = (IHttpRequestInputStreamFeature)feature;

        // Assert/Act
        Assert.Equal(ReadEntityBodyMode.None, adapterFeature.Mode);
        Assert.Throws<InvalidOperationException>(() => adapterFeature.InputStream);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Prebuffer(bool canSeek)
    {
        // Arrange
        using var stream = new TestStream(canSeek);

        var other = new Mock<IHttpRequestFeature>();
        other.Setup(o => o.Body).Returns(stream);

        using var feature = new HttpRequestInputStreamFeature(other.Object);

        var adapterFeature = (IHttpRequestInputStreamFeature)feature;
        await adapterFeature.BufferInputStreamAsync(default);

        // Act
        var mode1 = adapterFeature.Mode;
        var inputStream = adapterFeature.InputStream;
        var mode2 = adapterFeature.Mode;

        // Assert
        Assert.Equal(0, inputStream.Length);
        Assert.True(inputStream.CanSeek);
        Assert.Equal(ReadEntityBodyMode.Classic, mode1);
        Assert.Equal(mode1, mode2);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetBufferless(bool canSeek)
    {
        // Arrange
        using var stream = new TestStream(canSeek);

        var other = new Mock<IHttpRequestFeature>();
        other.Setup(o => o.Body).Returns(stream);

        using var feature = new HttpRequestInputStreamFeature(other.Object);
        var adapterFeature = (IHttpRequestInputStreamFeature)feature;

        // Act
        var bufferelessStream = adapterFeature.GetBufferlessInputStream();
        var mode = adapterFeature.Mode;

        // Assert
        Assert.Same(stream, bufferelessStream);
        Assert.Equal(canSeek, bufferelessStream.CanSeek);
        Assert.Equal(ReadEntityBodyMode.Bufferless, mode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetBuffered(bool canSeek)
    {
        // Arrange
        using var stream = new TestStream(canSeek);

        var other = new Mock<IHttpRequestFeature>();
        other.Setup(o => o.Body).Returns(stream);

        using var feature = new HttpRequestInputStreamFeature(other.Object);
        var adapterFeature = (IHttpRequestInputStreamFeature)feature;

        // Act
        var bufferedStream = adapterFeature.GetBufferedInputStream();
        var mode = adapterFeature.Mode;

        // Assert
        Assert.True(bufferedStream.CanSeek);
        Assert.Equal(ReadEntityBodyMode.Buffered, mode);
    }

    private sealed class TestStream : MemoryStream
    {
        public TestStream(bool canSeek) => CanSeek = canSeek;

        public override bool CanSeek { get; }

        public override long Length => throw new InvalidOperationException();
    }
}
