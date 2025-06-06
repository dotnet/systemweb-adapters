// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Test arguments")]
public class BinarySessionSerializerTests
{
    [InlineData(new byte[] { 1, 2, 105, 100, 0, 0, 0, 0, 0, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 0, 0, 0, 0, 0xFF })]
    [Theory]
    public async Task SerializeEmpty(byte[] data)
    {
        // Arrange
        var serializer = CreateSerializer();
        using var ms = new MemoryStream();

        var state = new Mock<ISessionState>();
        state.Setup(s => s.SessionID).Returns("id");

        // Act
        await serializer.SerializeAsync(state.Object, SessionSerializerContext.Get(data[0]), ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), data);
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 0, 0, 0, 0, 0, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 0, 0, 0, 0, 0xFF })]
    [Theory]
    public async Task DeserializeEmpty(byte[] data)
    {
        // Arrange
        using var ms = new MemoryStream(data);

        var serializer = CreateSerializer();

        // Act
        var result = await serializer.DeserializeAsync(ms, default);

        // Assert
        Assert.Equal("id", result!.SessionID);
        Assert.False(result.IsReadOnly);
        Assert.False(result.IsAbandoned);
        Assert.False(result.IsNewSession);
        Assert.Equal(0, result.Timeout);
        Assert.Equal(0, result.Count);
        Assert.Empty(result.Keys);
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 1, 0, 0, 0, 0, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 1, 0, 0, 0, 0xFF })]
    [Theory]
    public async Task SerializeIsNewSession(byte[] data)
    {
        // Arrange
        var serializer = CreateSerializer();
        using var ms = new MemoryStream();

        var state = new Mock<ISessionState>();
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.IsNewSession).Returns(true);

        // Act
        await serializer.SerializeAsync(state.Object, SessionSerializerContext.Get(data[0]), ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), data);
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 1, 0, 0, 0, 0, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 1, 0, 0, 0, 0XFF })]
    [Theory]
    public async Task DeserializeIsNewSession(byte[] data)
    {
        // Arrange
        using var ms = new MemoryStream(data);

        var serializer = CreateSerializer();

        // Act
        var result = await serializer.DeserializeAsync(ms, default);

        // Assert
        Assert.Equal("id", result!.SessionID);
        Assert.False(result.IsReadOnly);
        Assert.False(result.IsAbandoned);
        Assert.True(result.IsNewSession);
        Assert.Equal(0, result.Timeout);
        Assert.Equal(0, result.Count);
        Assert.Empty(result.Keys);
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 0, 1, 0, 0, 0, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 0, 1, 0, 0, 0xFF })]
    [Theory]
    public async Task SerializeIsAbandoned(byte[] data)
    {
        // Arrange
        var serializer = CreateSerializer();
        using var ms = new MemoryStream();

        var state = new Mock<ISessionState>();
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.IsAbandoned).Returns(true);

        // Act
        await serializer.SerializeAsync(state.Object, SessionSerializerContext.Get(data[0]), ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), data);
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 0, 1, 0, 0, 0, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 0, 1, 0, 0, 0xFF })]
    [Theory]
    public async Task DeserializeIsAbandoned(byte[] data)
    {
        // Arrange
        using var ms = new MemoryStream(data);

        var serializer = CreateSerializer();

        // Act
        var result = await serializer.DeserializeAsync(ms, default);

        // Assert
        Assert.Equal("id", result!.SessionID);
        Assert.False(result.IsReadOnly);
        Assert.True(result.IsAbandoned);
        Assert.False(result.IsNewSession);
        Assert.Equal(0, result.Timeout);
        Assert.Equal(0, result.Count);
        Assert.Empty(result.Keys);
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 0, 0, 1, 0, 0, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 0, 0, 1, 0, 0xFF })]
    [Theory]
    public async Task SerializeIsReadOnly(byte[] data)
    {
        // Arrange
        var serializer = CreateSerializer();
        using var ms = new MemoryStream();

        var state = new Mock<ISessionState>();
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.IsReadOnly).Returns(true);

        // Act
        await serializer.SerializeAsync(state.Object, SessionSerializerContext.Get(data[0]), ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), data);
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 0, 0, 1, 0, 0, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 0, 0, 1, 0, 0xFF })]
    [Theory]
    public async Task DeserializeIsReadOnly(byte[] data)
    {
        // Arrange
        using var ms = new MemoryStream(data);

        var serializer = CreateSerializer();

        // Act
        var result = await serializer.DeserializeAsync(ms, default);

        // Assert
        Assert.Equal("id", result!.SessionID);
        Assert.True(result.IsReadOnly);
        Assert.False(result.IsAbandoned);
        Assert.False(result.IsNewSession);
        Assert.Equal(0, result.Timeout);
        Assert.Equal(0, result.Count);
        Assert.Empty(result.Keys);
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 0, 0, 1, 0, 0, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 0, 0, 1, 0, 0xFF })]
    [Theory]
    public async Task DeserializeIsReadOnlyEmptyNull(byte[] data)
    {
        // Arrange
        using var ms = new MemoryStream(data);

        var serializer = CreateSerializer();

        // Act
        var result = await serializer.DeserializeAsync(ms, default);

        // Assert
        Assert.Equal("id", result!.SessionID);
        Assert.True(result.IsReadOnly);
        Assert.False(result.IsAbandoned);
        Assert.False(result.IsNewSession);
        Assert.Equal(0, result.Timeout);
        Assert.Equal(0, result.Count);
        Assert.Empty(result.Keys);
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 0, 0, 0, 20, 0, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 0, 0, 0, 20, 0xFF })]
    [Theory]
    public async Task SerializeTimeout(byte[] data)
    {
        // Arrange
        var serializer = CreateSerializer();
        using var ms = new MemoryStream();

        var state = new Mock<ISessionState>();
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.Timeout).Returns(20);

        // Act
        await serializer.SerializeAsync(state.Object, SessionSerializerContext.Get(data[0]), ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), data);
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 0, 0, 0, 20, 0, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 0, 0, 0, 20, 0xFF })]
    [Theory]
    public async Task DeserializeTimeout(byte[] data)
    {
        // Arrange
        using var ms = new MemoryStream(data);

        var serializer = CreateSerializer();

        // Act
        var result = await serializer.DeserializeAsync(ms, default);

        // Assert
        Assert.Equal("id", result!.SessionID);
        Assert.False(result.IsReadOnly);
        Assert.False(result.IsAbandoned);
        Assert.False(result.IsNewSession);
        Assert.Equal(20, result.Timeout);
        Assert.Equal(0, result.Count);
        Assert.Empty(result.Keys);
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 42, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 42, 0xFF })]
    [Theory]
    public async Task Serialize1Key(byte[] data)
    {
        // Arrange
        var obj = new object();
        var state = new Mock<ISessionState>();
        state.Setup(s => s["key1"]).Returns(obj);
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.Keys).Returns(new[] { "key1" });
        state.Setup(s => s.Count).Returns(1);

        var keySerializer = new Mock<ISessionKeySerializer>();
        var bytes = new byte[] { 42 };
        keySerializer.Setup(k => k.TrySerialize("key1", obj, out bytes)).Returns(true);

        var serializer = CreateSerializer(keySerializer.Object);
        using var ms = new MemoryStream();

        // Act
        await serializer.SerializeAsync(state.Object, SessionSerializerContext.Get(data[0]), ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), data);
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 0, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 0, 0xFF })]
    [Theory]
    public async Task Serialize1KeyNull(byte[] data)
    {
        // Arrange
        var obj = default(object);
        var state = new Mock<ISessionState>();
        state.Setup(s => s["key1"]).Returns(obj);
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.Keys).Returns(new[] { "key1" });
        state.Setup(s => s.Count).Returns(1);

        var keySerializer = new Mock<ISessionKeySerializer>();
        var bytes = new byte[] { 0 };
        keySerializer.Setup(k => k.TrySerialize("key1", obj, out bytes)).Returns(true);

        var serializer = CreateSerializer(keySerializer.Object);
        using var ms = new MemoryStream();

        // Act
        await serializer.SerializeAsync(state.Object, SessionSerializerContext.Get(data[0]), ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), data);
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 0, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 0, 0xFF })]
    [Theory]
    public async Task Deserialize1KeyNull(byte[] data)
    {
        // Arrange
        var obj = new object();
        var value = new byte[] { 0 };

        var keySerializer = new Mock<ISessionKeySerializer>();
        keySerializer.Setup(k => k.TryDeserialize("key1", value, out obj)).Returns(true);

        var serializer = CreateSerializer(keySerializer.Object);

        // Act
        var result = await serializer.DeserializeAsync(new MemoryStream(data), default);

        // Assert
        Assert.Equal("id", result!.SessionID);
        Assert.False(result.IsReadOnly);
        Assert.False(result.IsAbandoned);
        Assert.False(result.IsNewSession);
        Assert.Equal(0, result.Timeout);
        Assert.Equal(1, result.Count);
        Assert.Same(obj, result["key1"]);
        Assert.Collection(result.Keys, k => Assert.Equal("key1", k));
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 0, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 0, 0xFF })]
    [Theory]
    public async Task Deserialize1KeyV1(byte[] data)
    {
        // Arrange
        var obj = new object();
        var keySerializer = new Mock<ISessionKeySerializer>();
        keySerializer.Setup(k => k.TryDeserialize("key1", Array.Empty<byte>(), out obj)).Returns(true);

        using var ms = new MemoryStream(data);

        var serializer = CreateSerializer(keySerializer.Object);

        // Act
        var result = await serializer.DeserializeAsync(ms, default);

        // Assert
        Assert.Equal("id", result!.SessionID);
        Assert.False(result.IsReadOnly);
        Assert.False(result.IsAbandoned);
        Assert.False(result.IsNewSession);
        Assert.Equal(0, result.Timeout);
        Assert.Equal(1, result.Count);
        Assert.Equal(result.Keys, new[] { "key1" });
        Assert.Equal(obj, result["key1"]);
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 0, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 0, 0xFF })]
    [Theory]
    public async Task Serialize1KeyNullable(byte[] data)
    {
        // Arrange
        var obj = (int?)5;
        var state = new Mock<ISessionState>();
        state.Setup(s => s["key1"]).Returns(obj);
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.Keys).Returns(new[] { "key1" });
        state.Setup(s => s.Count).Returns(1);

        var keySerializer = new Mock<ISessionKeySerializer>();
        var bytes = new byte[] { 0 };
        keySerializer.Setup(k => k.TrySerialize("key1", obj, out bytes)).Returns(true);

        var serializer = CreateSerializer(keySerializer.Object);
        using var ms = new MemoryStream();

        // Act
        await serializer.SerializeAsync(state.Object, SessionSerializerContext.Get(data[0]), ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), data);
    }

    [InlineData(new byte[] { 1, 2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 42, 0 })]
    [InlineData(new byte[] { 2, 2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 42, 0xFF })]
    [Theory]
    public async Task Deserialize1Key(byte[] data)
    {
        // Arrange
        var obj = new object();
        var bytes = new byte[] { 42 };
        var keySerializer = new Mock<ISessionKeySerializer>();
        keySerializer.Setup(k => k.TryDeserialize("key1", bytes, out obj)).Returns(true);

        using var ms = new MemoryStream(data);

        var serializer = CreateSerializer(keySerializer.Object);

        // Act
        var result = await serializer.DeserializeAsync(ms, default);

        // Assert
        Assert.Equal("id", result!.SessionID);
        Assert.False(result.IsReadOnly);
        Assert.False(result.IsAbandoned);
        Assert.False(result.IsNewSession);
        Assert.Equal(0, result.Timeout);
        Assert.Equal(1, result.Count);
        Assert.Equal(result.Keys, new[] { "key1" });
        Assert.Equal(obj, result["key1"]);
    }

    private static BinarySessionSerializer CreateSerializer(ISessionKeySerializer? keySerializer = null)
    {
        keySerializer ??= new Mock<ISessionKeySerializer>().Object;
        var logger = new Mock<ILogger<BinarySessionSerializer>>();

        var optionContainer = new Mock<IOptions<SessionSerializerOptions>>();
        optionContainer.Setup(o => o.Value).Returns(new SessionSerializerOptions());

        return new BinarySessionSerializer(new Composite(keySerializer), optionContainer.Object, logger.Object);
    }

    private sealed class Composite : ICompositeSessionKeySerializer
    {
        private readonly ISessionKeySerializer _serializer;

        public Composite(ISessionKeySerializer serializer)
        {
            _serializer = serializer;
        }

        public bool TryDeserialize(string key, byte[] bytes, out object? obj)
            => _serializer.TryDeserialize(key, bytes, out obj);

        public bool TrySerialize(string key, object? value, out byte[] bytes)
            => _serializer.TrySerialize(key, value, out bytes);
    }
}
