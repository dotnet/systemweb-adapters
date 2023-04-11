// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization.Tests;

public class BinarySessionSerializerTests
{
    [Fact]
    public async Task SerializeEmpty()
    {
        // Arrange
        var serializer = CreateSerializer();
        using var ms = new MemoryStream();

        var state = new Mock<ISessionState>();
        state.Setup(s => s.SessionID).Returns("id");

        // Act
        await serializer.SerializeAsync(state.Object, ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), new byte[] { 1, 2, 105, 100, 0, 0, 0, 0, 0, 0 });
    }

    [Fact]
    public async Task DeserializeEmpty()
    {
        // Arrange
        var data = new byte[] { 1, 2, 105, 100, 0, 0, 0, 0, 0, 0 };
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

    [Fact]
    public async Task SerializeIsNewSession()
    {
        // Arrange
        var serializer = CreateSerializer();
        using var ms = new MemoryStream();

        var state = new Mock<ISessionState>();
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.IsNewSession).Returns(true);

        // Act
        await serializer.SerializeAsync(state.Object, ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), new byte[] { 1, 2, 105, 100, 1, 0, 0, 0, 0, 0 });
    }

    [Fact]
    public async Task DeserializeIsNewSession()
    {
        // Arrange
        var data = new byte[] { 1, 2, 105, 100, 1, 0, 0, 0, 0, 0 };
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

    [Fact]
    public async Task SerializeIsAbandoned()
    {
        // Arrange
        var serializer = CreateSerializer();
        using var ms = new MemoryStream();

        var state = new Mock<ISessionState>();
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.IsAbandoned).Returns(true);

        // Act
        await serializer.SerializeAsync(state.Object, ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), new byte[] { 1, 2, 105, 100, 0, 1, 0, 0, 0, 0 });
    }

    [Fact]
    public async Task DeserializeIsAbandoned()
    {
        // Arrange
        var data = new byte[] { 1, 2, 105, 100, 0, 1, 0, 0, 0, 0 };
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

    [Fact]
    public async Task SerializeIsReadOnly()
    {
        // Arrange
        var serializer = CreateSerializer();
        using var ms = new MemoryStream();

        var state = new Mock<ISessionState>();
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.IsReadOnly).Returns(true);

        // Act
        await serializer.SerializeAsync(state.Object, ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), new byte[] { 1, 2, 105, 100, 0, 0, 1, 0, 0, 0 });
    }

    [Fact]
    public async Task DeserializeIsReadOnly()
    {
        // Arrange
        var data = new byte[] { 1, 2, 105, 100, 0, 0, 1, 0, 0, 0 };
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

    [Fact]
    public async Task SerializeTimeout()
    {
        // Arrange
        var serializer = CreateSerializer();
        using var ms = new MemoryStream();

        var state = new Mock<ISessionState>();
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.Timeout).Returns(20);

        // Act
        await serializer.SerializeAsync(state.Object, ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), new byte[] { 1, 2, 105, 100, 0, 0, 0, 20, 0, 0 });
    }

    [Fact]
    public async Task DeserializeTimeout()
    {
        // Arrange
        var data = new byte[] { 1, 2, 105, 100, 0, 0, 0, 20, 0, 0 };
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

    [Fact]
    public async Task Serialize1Key()
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
        await serializer.SerializeAsync(state.Object, ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), new byte[] { 1, 2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 42, 0 });
    }

    [Fact]
    public async Task Deserialize1Key()
    {
        // Arrange
        var obj = new object();
        var bytes = new byte[] { 42 };
        var keySerializer = new Mock<ISessionKeySerializer>();
        keySerializer.Setup(k => k.TryDeserialize("key1", bytes, out obj)).Returns(true);

        var data = new byte[] { 1, 2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 42, 0 };
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

    [Fact]
    public async Task MultipleSerializersSerialize()
    {
        // Arrange
        var obj1 = new object();
        var obj2 = new object();
        var state = new Mock<ISessionState>();
        state.Setup(s => s["key1"]).Returns(obj1);
        state.Setup(s => s["key2"]).Returns(obj2);
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.Keys).Returns(new[] { "key1", "key2" });
        state.Setup(s => s.Count).Returns(2);

        var keySerializer1 = new Mock<ISessionKeySerializer>();
        var bytes1 = new byte[] { 42 };
        keySerializer1.Setup(k => k.TrySerialize("key1", obj1, out bytes1)).Returns(true);

        var keySerializer2 = new Mock<ISessionKeySerializer>();
        var bytes2 = new byte[] { 43 };
        keySerializer2.Setup(k => k.TrySerialize("key2", obj2, out bytes2)).Returns(true);

        var serializer = CreateSerializer(keySerializer1.Object, keySerializer2.Object);
        using var ms = new MemoryStream();

        // Act
        await serializer.SerializeAsync(state.Object, ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), new byte[] { 1, 2, 105, 100, 0, 0, 0, 0, 2, 4, 107, 101, 121, 49, 1, 42, 4, 107, 101, 121, 50, 1, 43, 0 });
    }

    [Fact]
    public async Task MultipleSerializersDeserialize()
    {
        // Arrange
        var obj1 = new object();
        var bytes1 = new byte[] { 42 };
        var keySerializer1 = new Mock<ISessionKeySerializer>();
        keySerializer1.Setup(k => k.TryDeserialize("key1", bytes1, out obj1)).Returns(true);

        var obj2 = new object();
        var bytes2 = new byte[] { 43 };
        var keySerializer2 = new Mock<ISessionKeySerializer>();
        keySerializer1.Setup(k => k.TryDeserialize("key2", bytes2, out obj2)).Returns(true);

        var data = new byte[] { 1, 2, 105, 100, 0, 0, 0, 0, 2, 4, 107, 101, 121, 49, 1, 42, 4, 107, 101, 121, 50, 1, 43, 0 };
        using var ms = new MemoryStream(data);

        var serializer = CreateSerializer(keySerializer1.Object, keySerializer2.Object);

        // Act
        var result = await serializer.DeserializeAsync(ms, default);

        // Assert
        Assert.Equal("id", result!.SessionID);
        Assert.False(result.IsReadOnly);
        Assert.False(result.IsAbandoned);
        Assert.False(result.IsNewSession);
        Assert.Equal(0, result.Timeout);
        Assert.Equal(2, result.Count);
        Assert.Equal(result.Keys, new[] { "key1", "key2" });
        Assert.Equal(obj1, result["key1"]);
        Assert.Equal(obj2, result["key2"]);
    }

    private static BinarySessionSerializer CreateSerializer(params ISessionKeySerializer[] keySerializers)
    {
        if (keySerializers.Length == 0)
        {
            keySerializers = new[] { new Mock<ISessionKeySerializer>().Object };
        }

        var logger = new Mock<ILogger<BinarySessionSerializer>>();

        var optionContainer = new Mock<IOptions<SessionSerializerOptions>>();
        optionContainer.Setup(o => o.Value).Returns(new SessionSerializerOptions());

        return new BinarySessionSerializer(keySerializers, optionContainer.Object, logger.Object);
    }
}
