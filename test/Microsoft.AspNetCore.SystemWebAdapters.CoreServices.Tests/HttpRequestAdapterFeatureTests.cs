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
    private const int DefaultThreshold = 500;

    [Fact]
    public void InputStream()
    {
        // Arrange
        var stream = new Mock<Stream>();

        var other = new Mock<IHttpRequestFeature>();
        other.Setup(o => o.Body).Returns(stream.Object);

        var feature = new HttpRequestAdapterFeature(other.Object, DefaultThreshold, default);
        var adapterFeature = (IHttpRequestAdapterFeature)feature;

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
        var stream = new TestStream(canSeek);

        var other = new Mock<IHttpRequestFeature>();
        other.Setup(o => o.Body).Returns(stream);

        var feature = new HttpRequestAdapterFeature(other.Object, DefaultThreshold, default);
        await feature.PreBufferInputStreamAsync(default);

        var adapterFeature = (IHttpRequestAdapterFeature)feature;

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
        var stream = new TestStream(canSeek);

        var other = new Mock<IHttpRequestFeature>();
        other.Setup(o => o.Body).Returns(stream);

        var feature = new HttpRequestAdapterFeature(other.Object, DefaultThreshold, default);
        var adapterFeature = (IHttpRequestAdapterFeature)feature;

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
        var stream = new TestStream(canSeek);

        var other = new Mock<IHttpRequestFeature>();
        other.Setup(o => o.Body).Returns(stream);

        var feature = new HttpRequestAdapterFeature(other.Object, DefaultThreshold, default);
        var adapterFeature = (IHttpRequestAdapterFeature)feature;

        // Act
        var bufferedStream = adapterFeature.GetBufferedInputStream();
        var mode = adapterFeature.Mode;

        // Assert
        Assert.True(bufferedStream.CanSeek);
        Assert.Equal(ReadEntityBodyMode.Buffered, mode);
    }

    private class TestStream : MemoryStream
    {
        public TestStream(bool canSeek) => CanSeek = canSeek;

        public override bool CanSeek { get; }

        public override long Length => throw new InvalidOperationException();
    }
}
