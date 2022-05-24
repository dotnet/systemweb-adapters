// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

public class JsonSessionStateSerializationTests
{
    private readonly Fixture _fixture;

    public JsonSessionStateSerializationTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void NewSession()
    {
        // Arrange
        const string PayLoad = @"{
    ""n"": true,
}";
        var serializer = CreateSerializer();

        // Act
        var result = serializer.Deserialize(PayLoad);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result!.Count);
        Assert.True(result.IsNewSession);
    }

    [Fact]
    public void SingleValueInt()
    {
        // Arrange
        const string PayLoad = @"{
    ""id"": ""5"",
    ""v"": {
        ""Key1"": 5
    }
}";

        var serializer = CreateSerializer(new()
        {
            KnownKeys =
            {
                { "Key1", typeof(int) }
            }
        });

        // Act
        var result = serializer.Deserialize(PayLoad);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.Count);
        AssertKey(result, "Key1", 5);
    }

    [Fact]
    public void Roundtrip()
    {
        // Arrange
        const string PayLoad = @"{
    ""id"": ""5"",
    ""v"": {
        ""Key1"": 5
    }
}";

        var serializer = CreateSerializer(new()
        {
            KnownKeys =
            {
                { "Key1", typeof(int) }
            },
            Indented = true
        });

        var sessionState = serializer.Deserialize(PayLoad);

        var result = new MemoryStream();

        // Act
        var byteResult = serializer.Serialize(sessionState!);
        var str = Encoding.UTF8.GetString(byteResult);

        // Assert
        const string Expected = @"{
  ""id"": ""5"",
  ""v"": {
    ""Key1"": 5
  }
}";

        Assert.Equal(Expected, str);
    }

    [Fact]
    public void MultipleValuesPrimitive()
    {
        // Arrange
        const string PayLoad = @"{
    ""id"": ""5"",
    ""v"": {
        ""Key1"": 5,
        ""Key2"": ""hello""
    }
}";

        var serializer = CreateSerializer(new()
        {
            KnownKeys =
            {
                { "Key1", typeof(int) },
                { "Key2", typeof(string) }
            },
        });

        // Act
        var result = serializer.Deserialize(PayLoad);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);

        AssertKey(result, "Key1", 5);
        AssertKey(result, "Key2", "hello");
    }

    [Fact]
    public void ComplexObject()
    {
        // Arrange
        const string PayLoad = @"{
    ""id"": ""5"",
    ""v"": {
        ""Key1"": {
            ""IntKey"": 5,
            ""StringKey"": ""hello""
        }
    }
}";

        var serializer = CreateSerializer(new()
        {
            KnownKeys =
            {
                { "Key1", typeof(SomeObject) },
            },
        });

        // Act
        var result = serializer.Deserialize(PayLoad);

        // Assert
        var obj = Assert.IsType<SomeObject>(result!["Key1"]);
        Assert.Equal(5, obj.IntKey);
        Assert.Equal("hello", obj.StringKey);
    }

    [Fact]
    public void DeserializeUnknownKeysThrows()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var data = _fixture.CreateMany<byte>().ToArray();
        var serializer = CreateSerializer(new() { ThrowOnUnknownSessionKey = true });

        // Act
        var exception = Assert.Throws<UnknownSessionKeyException>(() => serializer.Deserialize(key, data));

        // Assert
        Assert.Collection(exception.UnknownKeys, e => Assert.Equal(key, e));
    }

    [Fact]
    public void SerializeUnknownKeysThrows()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var serializer = CreateSerializer(new() { ThrowOnUnknownSessionKey = true });

        // Act
        var exception = Assert.Throws<UnknownSessionKeyException>(() => serializer.Serialize(key, new object()));

        // Assert
        Assert.Collection(exception.UnknownKeys, e => Assert.Equal(key, e));
    }

    [Fact]
    public void SerializeUnknownKeysFullPayloadThrows()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var value = _fixture.Create<int>();
        var keys = new[] { key };

        var state = new Mock<ISessionState>();
        state.Setup(s => s.Keys).Returns(keys);
        state.Setup(s => s[key]).Returns(value);

        var serializer = CreateSerializer(new() { ThrowOnUnknownSessionKey = true });

        // Act
        var result = Assert.Throws<UnknownSessionKeyException>(() => serializer.Serialize(state.Object));

        // Assert
        Assert.Equal(keys, result.UnknownKeys);
    }

    [Fact]
    public void SerializeUnknownKeysFullPayload()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var value = _fixture.Create<int>();
        var keys = new[] { key };

        var state = new Mock<ISessionState>();
        state.Setup(s => s.Keys).Returns(keys);
        state.Setup(s => s[key]).Returns(value);

        var serializer = CreateSerializer(new() { Indented = true });

        // Act
        var result = serializer.Serialize(state.Object);
        var str = Encoding.UTF8.GetString(result);

        // Assert
        var expected = @$"{{
  ""v"": {{}},
  ""u"": [
    ""{key}""
  ]
}}";
        Assert.Equal(expected, str);
    }

    [Fact]
    public void DeserializeUnknownKeysFullPayloadThrows()
    {
        // Arrange
        const string PayLoad = @"{
    ""id"": ""5"",
    ""v"": {
        ""Key1"": 5,
        ""Key2"": 1
    }
}";

        var serializer = CreateSerializer(new() { ThrowOnUnknownSessionKey = true });

        // Act
        var result = Assert.Throws<UnknownSessionKeyException>(() => serializer.Deserialize(PayLoad));

        // Assert
        Assert.Equal(new[] { "Key1", "Key2" }, result.UnknownKeys);
    }

    [Fact]
    public void DeserializeUnknownKeysFullPayload()
    {
        // Arrange
        const string PayLoad = @"{
    ""id"": ""5"",
    ""v"": {
        ""Key1"": 5,
        ""Key2"": 1,
        ""Key3"": 6,
    }
}";

        var serializer = CreateSerializer(new()
        {
            ThrowOnUnknownSessionKey = false,
            KnownKeys =
            {
                { "Key1", typeof(int) },
                { "Key3", typeof(int) }
            }
        });

        // Act
        var result = serializer.Deserialize(PayLoad)!;

        // Assert
        Assert.Collection(GetUnknownKeys(result), k => Assert.Equal("Key2", k));
        AssertKey(result, "Key1", 5);
        AssertKey(result, "Key3", 6);
    }

    [Fact]
    public void DeserializeUnknownKeysFullPayloadComplexObject()
    {
        // Arrange
        const string PayLoad = @"{
    ""id"": ""5"",
    ""v"": {
        ""Key1"": 5,
        ""Key2"": {
            ""Item"": 5,
        },
        ""Key3"": 6,
    }
}";

        var serializer = CreateSerializer(new()
        {
            ThrowOnUnknownSessionKey = false,
            KnownKeys =
            {
                { "Key1", typeof(int) },
                { "Key3", typeof(int) }
            }
        });

        // Act
        var result = serializer.Deserialize(PayLoad)!;

        // Assert
        Assert.Collection(GetUnknownKeys(result), k => Assert.Equal("Key2", k));
        AssertKey(result, "Key1", 5);
        AssertKey(result, "Key3", 6);
    }

    [Fact]
    public void DeserializeUnknownKeysFullPayloadArray()
    {
        // Arrange
        const string PayLoad = @"{
    ""id"": ""5"",
    ""v"": {
        ""Key1"": 5,
        ""Key2"": [1,2,3],
        ""Key3"": 6,
    }
}";

        var serializer = CreateSerializer(new()
        {
            ThrowOnUnknownSessionKey = false,
            KnownKeys =
            {
                { "Key1", typeof(int) },
                { "Key3", typeof(int) }
            }
        });

        // Act
        var result = serializer.Deserialize(PayLoad)!;

        // Assert
        Assert.Collection(GetUnknownKeys(result), k => Assert.Equal("Key2", k));
        AssertKey(result, "Key1", 5);
        AssertKey(result, "Key3", 6);
    }

    private static void AssertKey<T>(ISessionState? state, string key, T expected)
    {
        Assert.NotNull(state);
        var result = Assert.IsType<T>(state![key]);
        Assert.Equal(expected, result);
    }

    private static JsonSessionSerializer CreateSerializer(JsonSessionSerializerOptions? options = null)
    {
        var optionsProvider = new Mock<IOptions<JsonSessionSerializerOptions>>();
        optionsProvider.Setup(o => o.Value).Returns(options ?? new());

        return new JsonSessionSerializer(optionsProvider.Object, new Mock<ILogger<JsonSessionSerializer>>().Object);
    }

    private class SomeObject
    {
        public int IntKey { get; set; }

        public string StringKey { get; set; } = null!;
    }

    internal static IReadOnlyCollection<string>? GetUnknownKeys(ISessionState session)
        => ((SerializedSessionState)session).UnknownKeys; 
}
