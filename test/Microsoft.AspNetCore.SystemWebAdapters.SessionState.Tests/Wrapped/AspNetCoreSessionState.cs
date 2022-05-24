// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Wrapped.Tests;

public class AspNetCoreSessionStateTests
{
    private readonly Fixture _fixture;

    public AspNetCoreSessionStateTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetValue()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var value = _fixture.CreateMany<byte>().ToArray();
        var expected = new object();

        var session = new Mock<ISession>();
        session.Setup(s => s.TryGetValue(key, out value)).Returns(true);

        var serializer = new Mock<ISessionSerializer>();
        serializer.Setup(s => s.Deserialize(key, value)).Returns(expected);

        await using var state = new AspNetCoreSessionState(session.Object, serializer.Object, isReadOnly: false);

        // Act
        var result = state[key];

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task SetValue()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var bytes = _fixture.CreateMany<byte>().ToArray();
        var obj = new object();

        var session = new Mock<ISession>();

        var serializer = new Mock<ISessionSerializer>();
        serializer.Setup(s => s.Serialize(key, obj)).Returns(bytes);

        await using var state = new AspNetCoreSessionState(session.Object, serializer.Object, isReadOnly: false);

        // Act
        state[key] = obj;

        // Assert
        session.Verify(s => s.Set(key, bytes), Times.Once);
    }

    [Fact]
    public async Task SetReadOnly()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var obj = new object();

        var session = new Mock<ISession>();
        var serializer = new Mock<ISessionSerializer>();

        await using var state = new AspNetCoreSessionState(session.Object, serializer.Object, isReadOnly: true);

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => state[key] = obj);
    }

    [Fact]
    public async Task SessionId()
    {
        // Arrange
        var id = _fixture.Create<string>();

        var session = new Mock<ISession>();
        session.Setup(s => s.Id).Returns(id);
        var serializer = new Mock<ISessionSerializer>();

        await using var state = new AspNetCoreSessionState(session.Object, serializer.Object, isReadOnly: true);

        // Act
        var result = state.SessionID;

        // Assert
        Assert.Equal(id, result);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task IsReadOnly(bool isReadOnly)
    {
        // Arrange
        var session = new Mock<ISession>();
        var serializer = new Mock<ISessionSerializer>();

        await using var state = new AspNetCoreSessionState(session.Object, serializer.Object, isReadOnly: isReadOnly);

        // Act
        var result = state.IsReadOnly;

        // Assert
        Assert.Equal(isReadOnly, result);
    }

    [Fact]
    public async Task Count()
    {
        // Arrange
        const int Count = 10;
        var keys = _fixture.CreateMany<string>(10);

        var session = new Mock<ISession>();
        session.Setup(s => s.Keys).Returns(keys);
        var serializer = new Mock<ISessionSerializer>();

        await using var state = new AspNetCoreSessionState(session.Object, serializer.Object, isReadOnly: true);

        // Act
        var result = state.Count;

        // Assert
        Assert.Equal(Count, result);
    }

    [Fact]
    public async Task Keys()
    {
        // Arrange
        var keys = _fixture.CreateMany<string>(10);

        var session = new Mock<ISession>();
        session.Setup(s => s.Keys).Returns(keys);
        var serializer = new Mock<ISessionSerializer>();

        await using var state = new AspNetCoreSessionState(session.Object, serializer.Object, isReadOnly: true);

        // Act
        var result = state.Keys;

        // Assert
        Assert.Same(keys, result);
    }

    [Fact]
    public async Task Clear()
    {
        // Arrange
        var session = new Mock<ISession>();
        var serializer = new Mock<ISessionSerializer>();

        await using var state = new AspNetCoreSessionState(session.Object, serializer.Object, isReadOnly: false);

        // Act
        state.Clear();

        // Assert
        session.Verify(s => s.Clear(), Times.Once);
    }

    [Fact]
    public async Task ClearReadOnly()
    {
        // Arrange
        var session = new Mock<ISession>();
        var serializer = new Mock<ISessionSerializer>();

        await using var state = new AspNetCoreSessionState(session.Object, serializer.Object, isReadOnly: true);

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => state.Clear());
    }

    [Fact]
    public async Task Commit()
    {
        // Arrange
        var session = new Mock<ISession>();
        var serializer = new Mock<ISessionSerializer>();

        await using var state = new AspNetCoreSessionState(session.Object, serializer.Object, isReadOnly: false);

        // Act
        await state.CommitAsync(default);

        // Assert
        session.Verify(s => s.Clear(), Times.Never);
        session.Verify(s => s.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task CommitAbandoned()
    {
        // Arrange
        var session = new Mock<ISession>();
        var serializer = new Mock<ISessionSerializer>();

        await using var state = new AspNetCoreSessionState(session.Object, serializer.Object, isReadOnly: false)
        {
            IsAbandoned = true
        };

        // Act
        await state.CommitAsync(default);

        // Assert
        session.Verify(s => s.Clear(), Times.Once);
        session.Verify(s => s.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task CommitReadOnly()
    {
        // Arrange
        var session = new Mock<ISession>();
        var serializer = new Mock<ISessionSerializer>();

        await using var state = new AspNetCoreSessionState(session.Object, serializer.Object, isReadOnly: true);

        // Act/Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await state.CommitAsync(default));
    }

    [Fact]
    public async Task Remove()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var session = new Mock<ISession>();
        var serializer = new Mock<ISessionSerializer>();

        await using var state = new AspNetCoreSessionState(session.Object, serializer.Object, isReadOnly: false);

        // Act
        state.Remove(key);

        // Assert
        session.Verify(s => s.Remove(key), Times.Once);
    }

    [Fact]
    public async Task RemoveReadOnly()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var session = new Mock<ISession>();
        var serializer = new Mock<ISessionSerializer>();

        await using var state = new AspNetCoreSessionState(session.Object, serializer.Object, isReadOnly: true);

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => state.Remove(key));
    }
}
