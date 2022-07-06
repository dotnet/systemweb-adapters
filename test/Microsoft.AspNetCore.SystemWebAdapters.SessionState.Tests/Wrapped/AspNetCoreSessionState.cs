// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    public void GetValue()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var value = _fixture.CreateMany<byte>().ToArray();
        var expected = new object();

        var session = new Mock<ISession>();
        session.Setup(s => s.TryGetValue(key, out value)).Returns(true);

        var serializer = new Mock<ISessionKeySerializer>();
        serializer.Setup(s => s.TryDeserialize(key, value, out expected)).Returns(true);

        using var state = CreateSesionState(session, serializer);

        // Act
        var result = state[key];

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SetValue()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var bytes = _fixture.CreateMany<byte>().ToArray();
        var obj = new object();

        var session = new Mock<ISession>();

        var serializer = new Mock<ISessionKeySerializer>();
        serializer.Setup(s => s.TrySerialize(key, obj, out bytes)).Returns(true);

        using var state = CreateSesionState(session, serializer);

        // Act
        state[key] = obj;

        // Assert
        session.Verify(s => s.Set(key, bytes), Times.Once);
    }

    [Fact]
    public void SetReadOnly()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var obj = new object();

        using var state = CreateSesionState(isReadOnly: true);

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => state[key] = obj);
    }

    [Fact]
    public void SessionId()
    {
        // Arrange
        var id = _fixture.Create<string>();

        var session = new Mock<ISession>();
        session.Setup(s => s.Id).Returns(id);

        using var state = CreateSesionState(session, isReadOnly: true);

        // Act
        var result = state.SessionID;

        // Assert
        Assert.Equal(id, result);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void IsReadOnly(bool isReadOnly)
    {
        // Arrange
        using var state = CreateSesionState(isReadOnly: isReadOnly);

        // Act
        var result = state.IsReadOnly;

        // Assert
        Assert.Equal(isReadOnly, result);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task IsReadOnlySessionManager(bool isReadOnly)
    {
        // Arrange
        using var state = await CreateSessionStateFromSessionManager(isReadOnly: isReadOnly);

        // Act
        var result = state.IsReadOnly;

        // Assert
        Assert.Equal(isReadOnly, result);
    }

    [Fact]
    public void Count()
    {
        // Arrange
        const int Count = 10;
        var keys = _fixture.CreateMany<string>(10);

        var session = new Mock<ISession>();
        session.Setup(s => s.Keys).Returns(keys);

        using var state = CreateSesionState(session, isReadOnly: true);

        // Act
        var result = state.Count;

        // Assert
        Assert.Equal(Count, result);
    }

    [Fact]
    public void Keys()
    {
        // Arrange
        var keys = _fixture.CreateMany<string>(10);

        var session = new Mock<ISession>();
        session.Setup(s => s.Keys).Returns(keys);

        using var state = CreateSesionState(session, isReadOnly: true);

        // Act
        var result = state.Keys;

        // Assert
        Assert.Same(keys, result);
    }

    [Fact]
    public void Clear()
    {
        // Arrange
        var session = new Mock<ISession>();

        using var state = CreateSesionState(session);

        // Act
        state.Clear();

        // Assert
        session.Verify(s => s.Clear(), Times.Once);
    }

    [Fact]
    public void ClearReadOnly()
    {
        // Arrange
        using var state = CreateSesionState(isReadOnly: true);

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => state.Clear());
    }

    [Fact]
    public async Task Commit()
    {
        // Arrange
        var session = new Mock<ISession>();

        using var state = CreateSesionState(session);

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
        var serializer = new Mock<ISessionKeySerializer>();

        using var state = CreateSesionState(session);

        state.IsAbandoned = true;

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
        var serializer = new Mock<ISessionKeySerializer>();

        using var state = CreateSesionState(isReadOnly: true);

        // Act/Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await state.CommitAsync(default));
    }

    [Fact]
    public void Remove()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var session = new Mock<ISession>();

        using var state = CreateSesionState(session);

        // Act
        state.Remove(key);

        // Assert
        session.Verify(s => s.Remove(key), Times.Once);
    }

    [Fact]
    public void RemoveReadOnly()
    {
        // Arrange
        var key = _fixture.Create<string>();

        using var state = CreateSesionState(isReadOnly: true);

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => state.Remove(key));
    }

    private static AspNetCoreSessionState CreateSesionState(Mock<ISession>? session = null, Mock<ISessionKeySerializer>? serializer = null, bool isReadOnly = false, bool throwOnUnknown = false)
    {
        session ??= new Mock<ISession>();
        serializer ??= new Mock<ISessionKeySerializer>();
        var loggerFactory = new Mock<ILoggerFactory>();

        return new AspNetCoreSessionState(session.Object, serializer.Object, loggerFactory.Object, isReadOnly: isReadOnly, throwOnUnknown: throwOnUnknown);
    }
    private static async Task<ISessionState> CreateSessionStateFromSessionManager(Mock<ISession>? session = null, Mock<ISessionKeySerializer>? serializer = null, bool isReadOnly = false, bool throwOnUnknown = false)
    {
        session ??= new Mock<ISession>();
        serializer ??= new Mock<ISessionKeySerializer>();
        var loggerFactory = new Mock<ILoggerFactory>();
        var httpContextCore = new Mock<HttpContextCore>();
        httpContextCore.Setup(s => s.Session).Returns(session.Object);
        var sessionSerializerOptions = new SessionSerializerOptions() {ThrowOnUnknownSessionKey = throwOnUnknown};
        var options = Options.Create(sessionSerializerOptions);

        var aspNetCoreSessionManager = new AspNetCoreSessionManager(serializer.Object, loggerFactory.Object, options);
        return await aspNetCoreSessionManager.CreateAsync(httpContextCore.Object, new SessionAttribute(){IsReadOnly = isReadOnly});
    }
}
