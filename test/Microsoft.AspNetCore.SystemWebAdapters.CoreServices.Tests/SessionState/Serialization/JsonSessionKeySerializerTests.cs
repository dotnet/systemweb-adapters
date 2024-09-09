// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Autofac.Extras.Moq;
using AutoFixture;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization.Tests;

public class JsonSessionKeySerializerTests
{
    private readonly Fixture _fixture;

    public JsonSessionKeySerializerTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void SerializeNothingRegistered()
    {
        // Arrange
        using var mock = AutoMock.GetLoose();
        var key = _fixture.Create<string>();

        var options = new JsonSessionSerializerOptions();
        mock.Mock<IOptions<JsonSessionSerializerOptions>>().Setup(o => o.Value).Returns(options);

        var serializer = mock.Create<JsonSessionKeySerializer>();

        // Act
        var result = serializer.TrySerialize(key, new Type1(), out var bytes);

        // Assert
        Assert.False(result);
        Assert.Empty(bytes);
    }

    [Fact]
    public void DeserializeNothingRegistered()
    {
        // Arrange
        using var mock = AutoMock.GetLoose();
        var key = _fixture.Create<string>();
        var value = _fixture.CreateMany<byte>().ToArray();

        var options = new JsonSessionSerializerOptions();
        mock.Mock<IOptions<JsonSessionSerializerOptions>>().Setup(o => o.Value).Returns(options);

        var serializer = mock.Create<JsonSessionKeySerializer>();

        // Act
        var result = serializer.TryDeserialize(key, value, out var obj);

        // Assert
        Assert.False(result);
        Assert.Null(obj);
    }

    [Fact]
    public void SerializeTypeRegistered()
    {
        // Arrange
        using var mock = AutoMock.GetLoose();
        var key = _fixture.Create<string>();
        var value = _fixture.CreateMany<byte>().ToArray();

        var options = new JsonSessionSerializerOptions
        {
            KnownKeys =
            {
                { key, typeof(Type1) },
            }
        };
        mock.Mock<IOptions<JsonSessionSerializerOptions>>().Setup(o => o.Value).Returns(options);

        var serializer = mock.Create<JsonSessionKeySerializer>();

        // Act
        var result = serializer.TrySerialize(key, new Type1(), out var bytes);

        // Assert
        Assert.True(result);
        Assert.Equal("{}"u8.ToArray(), bytes);
    }

    [Fact]
    public void HandleNullValues()
    {
        // Arrange
        using var mock = AutoMock.GetLoose();
        var key = _fixture.Create<string>();
        var value = _fixture.CreateMany<byte>().ToArray();

        var options = new JsonSessionSerializerOptions
        {
            KnownKeys =
            {
                { key, typeof(Type1) },
            }
        };
        mock.Mock<IOptions<JsonSessionSerializerOptions>>().Setup(o => o.Value).Returns(options);

        var serializer = mock.Create<JsonSessionKeySerializer>();

        // Act
        var serializeResult = serializer.TrySerialize(key, null, out var bytes);
        var deserializeResult = serializer.TryDeserialize(key, bytes, out var deserialized);

        // Assert
        Assert.True(serializeResult);
        Assert.True(deserializeResult);
        Assert.Equal("null"u8.ToArray(), bytes);
        Assert.Null(deserialized);
    }

    [Fact]
    public void SerializeDerivedTypeRegistered()
    {
        // Arrange
        using var mock = AutoMock.GetLoose();
        var key = _fixture.Create<string>();
        var value = _fixture.CreateMany<byte>().ToArray();

        var options = new JsonSessionSerializerOptions
        {
            KnownKeys =
            {
                { key, typeof(Type2) },
            }
        };
        mock.Mock<IOptions<JsonSessionSerializerOptions>>().Setup(o => o.Value).Returns(options);

        var serializer = mock.Create<JsonSessionKeySerializer>();

        // Act
        var result = serializer.TrySerialize(key, new Type2Derived(), out var bytes);

        // Assert
        Assert.False(result);
        Assert.Empty(bytes);
    }

    [Fact]
    public void DeserializeTypeRegistered()
    {
        // Arrange
        using var mock = AutoMock.GetLoose();
        var key = _fixture.Create<string>();
        var value = "{}"u8.ToArray();

        var options = new JsonSessionSerializerOptions
        {
            KnownKeys =
            {
                { key, typeof(Type1) },
            }
        };
        mock.Mock<IOptions<JsonSessionSerializerOptions>>().Setup(o => o.Value).Returns(options);

        var serializer = mock.Create<JsonSessionKeySerializer>();

        // Act
        var result = serializer.TryDeserialize(key, value, out var obj);

        // Assert
        Assert.True(result);
        Assert.NotNull(obj);
    }

    [Fact]
    public void SerializeIncorrectTypeRegistered()
    {
        // Arrange
        using var mock = AutoMock.GetLoose();
        var key = _fixture.Create<string>();

        var options = new JsonSessionSerializerOptions
        {
            KnownKeys =
            {
                { key, typeof(Type1) },
            }
        };
        mock.Mock<IOptions<JsonSessionSerializerOptions>>().Setup(o => o.Value).Returns(options);

        var serializer = mock.Create<JsonSessionKeySerializer>();

        // Act
        var result = serializer.TrySerialize(key, new Type2(), out var bytes);

        // Assert
        Assert.False(result);
        Assert.Empty(bytes);
    }

    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(100)]
    [Theory]
    public void PrimitiveSerializer(int primitive)
    {
        // Arrange
        using var mock = AutoMock.GetLoose();
        var key = _fixture.Create<string>();
        var value = _fixture.CreateMany<byte>().ToArray();

        var options = new JsonSessionSerializerOptions
        {
            KnownKeys =
            {
                { key, typeof(int) },
            }
        };
        mock.Mock<IOptions<JsonSessionSerializerOptions>>().Setup(o => o.Value).Returns(options);

        var serializer = mock.Create<JsonSessionKeySerializer>();

        // Act
        var serializeResult = serializer.TrySerialize(key, primitive, out var bytes);
        var deserializeResult = serializer.TryDeserialize(key, bytes, out var deserialized);

        // Assert
        Assert.True(serializeResult);
        Assert.Equal(Encoding.UTF8.GetBytes(primitive.ToString(CultureInfo.InvariantCulture)), bytes);
        Assert.True(deserializeResult);
        Assert.Equal(primitive, deserialized);
    }

    [Fact]
    public void PrimitiveNullableNotSupportedSerializer()
    {
        // Arrange
        using var mock = AutoMock.GetLoose();
        var key = _fixture.Create<string>();
        var value = _fixture.CreateMany<byte>().ToArray();

        var options = new JsonSessionSerializerOptions
        {
            KnownKeys =
            {
                { key, typeof(int) },
            }
        };
        mock.Mock<IOptions<JsonSessionSerializerOptions>>().Setup(o => o.Value).Returns(options);

        var serializer = mock.Create<JsonSessionKeySerializer>();

        // Act
        var result = serializer.TrySerialize(key, default, out var bytes);

        // Assert
        Assert.False(result);
    }

    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(100)]
    [Theory]
    public void PrimitiveNullableSerializer(int? primitive)
    {
        // Arrange
        using var mock = AutoMock.GetLoose();
        var key = _fixture.Create<string>();
        var value = _fixture.CreateMany<byte>().ToArray();

        var options = new JsonSessionSerializerOptions
        {
            KnownKeys =
            {
                { key, typeof(int?) },
            }
        };
        mock.Mock<IOptions<JsonSessionSerializerOptions>>().Setup(o => o.Value).Returns(options);

        var serializer = mock.Create<JsonSessionKeySerializer>();

        // Act
        var serializeResult = serializer.TrySerialize(key, primitive, out var bytes);
        var deserializeResult = serializer.TryDeserialize(key, bytes, out var deserialized);

        // Assert
        Assert.True(serializeResult);
        Assert.Equal(Encoding.UTF8.GetBytes(primitive.HasValue ? primitive.Value.ToString(CultureInfo.InvariantCulture) : "null"), bytes);
        Assert.True(deserializeResult);
        Assert.Equal(primitive, deserialized);
    }

    [Fact]
    public void CaseInsensitiveSessionKeys()
    {
        // Arrange
        var key1 = _fixture.Create<string>();
        var key2 = _fixture.Create<string>();

        var options = new JsonSessionSerializerOptions
        {
            KeyComparer = StringComparer.OrdinalIgnoreCase,
            KnownKeys =
            {
                { key1, typeof(Type1) },
                { key2, typeof(int) },
            }
        };

        // Act
        var result1 = options.KnownKeys.ContainsKey(key1.ToUpperInvariant());
        var result2 = options.KnownKeys.ContainsKey(key2);

        // Assert
        Assert.True(result1);
        Assert.True(result2);
    }

    [Fact]
    public void ChangeSessionKeysComparer()
    {
        // Arrange
        var key1 = _fixture.Create<string>();
        var key2 = _fixture.Create<string>();

        var options = new JsonSessionSerializerOptions
        {
            KnownKeys =
            {
                { key1, typeof(Type1) },
                { key2, typeof(int) },
            }
        };

        // Act
        var result1 = options.KnownKeys.ContainsKey(key1);
        var result2 = options.KnownKeys.ContainsKey(key2);
        var result3 = options.KnownKeys.ContainsKey(key2.ToUpperInvariant());

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.False(result3);

        // Re-arrange
        options.KeyComparer = StringComparer.OrdinalIgnoreCase;

        // Act
        result1 = options.KnownKeys.ContainsKey(key1);
        result2 = options.KnownKeys.ContainsKey(key2.ToUpperInvariant());

        // Assert
        Assert.True(result1);
        Assert.True(result2);
    }

    private sealed class Type1
    {
    }

    private class Type2
    {
    }

    private sealed class Type2Derived : Type2
    {
    }
}
