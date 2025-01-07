// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization.Tests;

public class BinarySessionSerializerTests
{
    private const byte ModeStateV1 = 1;
    private const byte ModeStateV2 = 2;

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task SerializeEmpty(bool trackChanges)
    {
        // Arrange
        var serializer = CreateSerializer(trackChanges);
        using var ms = new MemoryStream();

        var state = new Mock<ISessionState>();
        state.Setup(s => s.SessionID).Returns("id");

        // Act
        await serializer.SerializeAsync(state.Object, ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), AddFlags([2, 105, 100, 0, 0, 0, 0, 0, 0], trackChanges));
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task DeserializeEmpty(bool trackChanges)
    {
        // Arrange
        var data = AddFlags([2, 105, 100, 0, 0, 0, 0, 0, 0], trackChanges);
        using var ms = new MemoryStream(data);

        var serializer = CreateSerializer(trackChanges);

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

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task SerializeIsNewSession(bool trackChanges)
    {
        // Arrange
        var serializer = CreateSerializer(null, trackChanges);
        using var ms = new MemoryStream();

        var state = new Mock<ISessionState>();
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.IsNewSession).Returns(true);

        // Act
        await serializer.SerializeAsync(state.Object, ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), AddFlags([2, 105, 100, 1, 0, 0, 0, 0, 0], trackChanges));
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task DeserializeIsNewSession(bool trackChanges)
    {
        // Arrange
        var data = AddFlags([2, 105, 100, 1, 0, 0, 0, 0, 0], trackChanges);
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

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task SerializeIsAbandoned(bool trackChanges)
    {
        // Arrange
        var serializer = CreateSerializer(trackChanges);
        using var ms = new MemoryStream();

        var state = new Mock<ISessionState>();
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.IsAbandoned).Returns(true);

        // Act
        await serializer.SerializeAsync(state.Object, ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), AddFlags([2, 105, 100, 0, 1, 0, 0, 0, 0], trackChanges));
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task DeserializeIsAbandoned(bool trackChanges)
    {
        // Arrange
        var data = AddFlags([2, 105, 100, 0, 1, 0, 0, 0, 0], trackChanges);
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

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task SerializeIsReadOnly(bool trackChanges)
    {
        // Arrange
        var serializer = CreateSerializer(trackChanges);
        using var ms = new MemoryStream();

        var state = new Mock<ISessionState>();
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.IsReadOnly).Returns(true);

        // Act
        await serializer.SerializeAsync(state.Object, ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), AddFlags([2, 105, 100, 0, 0, 1, 0, 0, 0], trackChanges));
    }

    [InlineData(FlagOptions.None)]
    [InlineData(FlagOptions.Changes)]
    [InlineData(FlagOptions.NoChanges)]
    [Theory]
    public async Task DeserializeIsReadOnly(FlagOptions options)
    {
        // Arrange
        var data = AddFlags([2, 105, 100, 0, 0, 1, 0, 0, 0], options);

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

        if (options is FlagOptions.None or FlagOptions.NoChanges)
        {
            Assert.IsNotAssignableFrom<ISessionStateChangeset>(result);
        }
        else
        {
            Assert.IsAssignableFrom<ISessionStateChangeset>(result);
        }
    }

    [InlineData(FlagOptions.None)]
    [InlineData(FlagOptions.Changes)]
    [InlineData(FlagOptions.NoChanges)]
    [Theory]
    public async Task DeserializeIsReadOnlyEmptyNull(FlagOptions options)
    {
        // Arrange

        var data = AddFlags([2, 105, 100, 0, 0, 1, 0, 0, 0], options);
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

        if (options is FlagOptions.None or FlagOptions.NoChanges)
        {
            Assert.IsNotAssignableFrom<ISessionStateChangeset>(result);
        }
        else
        {
            Assert.IsAssignableFrom<ISessionStateChangeset>(result);
        }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task SerializeTimeout(bool trackChanges)
    {
        // Arrange
        var serializer = CreateSerializer(keySerializer: null, trackChanges: trackChanges);
        using var ms = new MemoryStream();

        var state = new Mock<ISessionState>();
        state.Setup(s => s.SessionID).Returns("id");
        state.Setup(s => s.Timeout).Returns(20);

        // Act
        await serializer.SerializeAsync(state.Object, ms, default);

        // Assert
        Assert.Equal(AddFlags([2, 105, 100, 0, 0, 0, 20, 0, 0], trackChanges), ms.ToArray());
    }

    [InlineData(FlagOptions.None)]
    [InlineData(FlagOptions.Changes)]
    [InlineData(FlagOptions.NoChanges)]
    [Theory]
    public async Task DeserializeTimeout(FlagOptions options)
    {
        // Arrange
        var data = AddFlags([2, 105, 100, 0, 0, 0, 20, 0, 0], options);
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

        if (options is FlagOptions.None or FlagOptions.NoChanges)
        {
            Assert.IsNotAssignableFrom<ISessionStateChangeset>(result);
        }
        else
        {
            Assert.IsAssignableFrom<ISessionStateChangeset>(result);
        }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task Serialize1Key(bool trackChanges)
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

        var serializer = CreateSerializer(keySerializer.Object, trackChanges);
        using var ms = new MemoryStream();

        // Act
        await serializer.SerializeAsync(state.Object, ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), AddFlags([2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 42, 0], trackChanges));
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task Serialize1KeyNull(bool trackChanges)
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

        var serializer = CreateSerializer(keySerializer.Object, trackChanges);
        using var ms = new MemoryStream();

        // Act
        await serializer.SerializeAsync(state.Object, ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), AddFlags([2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 0, 0], trackChanges));
    }

    [InlineData(FlagOptions.None)]
    [InlineData(FlagOptions.Changes)]
    [InlineData(FlagOptions.NoChanges)]
    [Theory]
    public async Task Deserialize1KeyNull(FlagOptions options)
    {
        // Arrange
        var data = AddFlags([2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 0, 0], options);
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

        if (options is FlagOptions.None or FlagOptions.NoChanges)
        {
            Assert.IsNotAssignableFrom<ISessionStateChangeset>(result);
        }
        else
        {
            Assert.IsAssignableFrom<ISessionStateChangeset>(result);
        }
    }

    [InlineData(FlagOptions.None)]
    [InlineData(FlagOptions.Changes)]
    [InlineData(FlagOptions.NoChanges)]
    [Theory]
    public async Task Deserialize1KeyV1(FlagOptions options)
    {
        // Arrange
        var obj = new object();
        var keySerializer = new Mock<ISessionKeySerializer>();
        keySerializer.Setup(k => k.TryDeserialize("key1", Array.Empty<byte>(), out obj)).Returns(true);

        var data = AddFlags([2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 0, 0], options);
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

        if (options is FlagOptions.None or FlagOptions.NoChanges)
        {
            Assert.IsNotAssignableFrom<ISessionStateChangeset>(result);
        }
        else
        {
            Assert.IsAssignableFrom<ISessionStateChangeset>(result);
        }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task Serialize1KeyNullable(bool trackChanges)
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

        var serializer = CreateSerializer(keySerializer.Object, trackChanges);
        using var ms = new MemoryStream();

        // Act
        await serializer.SerializeAsync(state.Object, ms, default);

        // Assert
        Assert.Equal(ms.ToArray(), AddFlags([2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 0, 0], trackChanges));
    }

    [InlineData(FlagOptions.None)]
    [InlineData(FlagOptions.Changes)]
    [InlineData(FlagOptions.NoChanges)]
    [Theory]
    public async Task Deserialize1Key(FlagOptions options)
    {
        // Arrange
        var obj = new object();
        var bytes = new byte[] { 42 };
        var keySerializer = new Mock<ISessionKeySerializer>();
        keySerializer.Setup(k => k.TryDeserialize("key1", bytes, out obj)).Returns(true);

        var data = AddFlags([2, 105, 100, 0, 0, 0, 0, 1, 4, 107, 101, 121, 49, 1, 42, 0], options);
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

        if (options is FlagOptions.None or FlagOptions.NoChanges)
        {
            Assert.IsNotAssignableFrom<ISessionStateChangeset>(result);
        }
        else
        {
            Assert.IsAssignableFrom<ISessionStateChangeset>(result);
        }
    }

    [Fact]
    public async Task RoundtripDoesntOverwrite()
    {
        // Arrange
        var obj1 = new object();
        var bytes1 = new byte[] { 42 };
        var obj2 = new object();
        var bytes2 = new byte[] { 43 };
        var keySerializer1 = new Mock<ISessionKeySerializer>();
        RegisterKey(keySerializer1, "key1", obj1, bytes1);

        var serializer1 = CreateSerializer(keySerializer1.Object, options => options.ThrowOnUnknownSessionKey = false);

        using var initialState = new TestState()
        {
            { "key1", obj1 },
            { "key2", obj2 },
        };

        // Act
        var state2 = await RoundtripAsync(serializer1, initialState);

        // Assert
        var changeset = Assert.IsAssignableFrom<ISessionStateChangeset>(state2);

        Assert.Collection(
            changeset.Changes,
            c =>
            {
                Assert.Equal("key1", c.Key);
                Assert.Equal(SessionItemChangeState.NoChange, c.State);
            },
            c =>
            {
                Assert.Equal("key2", c.Key);
                Assert.Equal(SessionItemChangeState.NoChange, c.State);
            });
    }

    private static async Task<ISessionState> RoundtripAsync(BinarySessionSerializer serializer, ISessionState state)
    {
        using var ms = new MemoryStream();
        await serializer.SerializeAsync(state, ms, default);
        ms.Position = 0;

        var result = await serializer.DeserializeAsync(ms, default);

        return result!;
    }

    private static void RegisterKey(Mock<ISessionKeySerializer> keySerializer, string name, object? obj, byte[] data)
    {
        keySerializer.Setup(k => k.TryDeserialize(name, data, out obj)).Returns(true);
        keySerializer.Setup(k => k.TrySerialize(name, obj, out data)).Returns(true);
    }

    private static BinarySessionSerializer CreateSerializer() => CreateSerializer(null);

    private static BinarySessionSerializer CreateSerializer(bool trackChanges)
        => CreateSerializer(null, trackChanges);

    private static BinarySessionSerializer CreateSerializer(ISessionKeySerializer? keySerializer)
        => CreateSerializer(keySerializer, _ => { });

    private static BinarySessionSerializer CreateSerializer(ISessionKeySerializer? keySerializer, bool trackChanges)
        => CreateSerializer(keySerializer, options => options.EnableChangeTracking = trackChanges);

    private static BinarySessionSerializer CreateSerializer(ISessionKeySerializer? keySerializer, Action<SessionSerializerOptions> optionsConfigure)
    {
        keySerializer ??= new Mock<ISessionKeySerializer>().Object;
        var logger = new Mock<ILogger<BinarySessionSerializer>>();

        var optionContainer = new Mock<IOptions<SessionSerializerOptions>>();
        var options = new SessionSerializerOptions();
        optionsConfigure?.Invoke(options);
        optionContainer.Setup(o => o.Value).Returns(options);

        return new BinarySessionSerializer(new Composite(keySerializer), optionContainer.Object, logger.Object);
    }

    public enum FlagOptions
    {
        None = 0,
        NoChanges = 1,
        Changes = 2,
    }

    private static byte[] AddFlags(byte[] data, bool trackChanges)
        => AddFlags(data, trackChanges ? FlagOptions.Changes : FlagOptions.None);

    private static byte[] AddFlags(byte[] data, FlagOptions options) => options switch
    {
        FlagOptions.None => [ModeStateV1, .. data],
        FlagOptions.Changes => [ModeStateV2, .. data, 1, 100, 0],
        FlagOptions.NoChanges => [ModeStateV2, .. data, 0],
        _ => throw new ArgumentOutOfRangeException(nameof(options)),
    };

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

    private sealed class TestState : ISessionState, IEnumerable<KeyValuePair<string, object>>
    {
        private readonly Dictionary<string, object> _items = [];

        public object? this[string key]
        {
            get => _items.TryGetValue(key, out var value) ? value : null;
            set
            {
                if (value is null)
                {
                    _items.Remove(key);
                }
                else
                {
                    _items[key] = value;
                }
            }
        }

        public void Add(string key, object value)
        {
            _items.Add(key, value);
        }

        public string SessionID => "id";

        public bool IsReadOnly => false;

        public int Timeout { get; set; }

        public bool IsNewSession => true;

        public int Count => _items.Count;

        public bool IsSynchronized => false;

        public object SyncRoot => _items;

        public bool IsAbandoned { get; set; }

        public IEnumerable<string> Keys => _items.Keys;

        public void Clear() => _items.Clear();
        public Task CommitAsync(CancellationToken token) => Task.CompletedTask;
        public void Dispose()
        {
        }

        public void Remove(string key) => _items.Remove(key);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
