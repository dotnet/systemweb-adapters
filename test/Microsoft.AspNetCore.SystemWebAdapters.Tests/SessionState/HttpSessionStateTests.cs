// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Web.SessionState;
using AutoFixture;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Tests;

public class HttpSessionStateTests
{
    private readonly Fixture _fixture;

    public HttpSessionStateTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void SessionId()
    {
        // Arrange
        var id = _fixture.Create<string>();

        var session = new Mock<ISessionState>();
        session.Setup(s => s.SessionID).Returns(id);

        var state = new HttpSessionState(session.Object);

        // Act
        var result = state.SessionID;

        // Assert
        Assert.Equal(id, result);
    }

    [Fact]
    public void Mode()
    {
        // Arrange
        var mode = _fixture.Create<SessionStateMode>();

        var session = new Mock<ISessionState>();

        var state = new HttpSessionState(session.Object, mode);

        // Act
        var result = state.Mode;

        // Assert
        Assert.Equal(mode, result);
    }

    [Fact]
    public void Count()
    {
        // Arrange
        var count = _fixture.Create<int>();

        var session = new Mock<ISessionState>();
        session.Setup(s => s.Count).Returns(count);

        var state = new HttpSessionState(session.Object);

        // Act
        var result = state.Count;

        // Assert
        Assert.Equal(count, result);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void IsReadOnly(bool isReadOnly)
    {
        // Arrange
        var session = new Mock<ISessionState>();
        session.Setup(s => s.IsReadOnly).Returns(isReadOnly);

        var state = new HttpSessionState(session.Object);

        // Act
        var result = state.IsReadOnly;

        // Assert
        Assert.Equal(isReadOnly, result);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void IsNewSession(bool isNewSession)
    {
        // Arrange
        var session = new Mock<ISessionState>();
        session.Setup(s => s.IsNewSession).Returns(isNewSession);

        var state = new HttpSessionState(session.Object);

        // Act
        var result = state.IsNewSession;

        // Assert
        Assert.Equal(isNewSession, result);
    }

    [Fact]
    public void TimeOut()
    {
        // Arrange
        var timeout1 = _fixture.Create<int>();
        var timeout2 = _fixture.Create<int>();

        var session = new Mock<ISessionState>();
        session.SetupProperty(s => s.Timeout);
        session.Object.Timeout = timeout1;

        var state = new HttpSessionState(session.Object);

        // Act
        var result = state.Timeout;
        state.Timeout = timeout2;

        // Assert
        Assert.Equal(timeout1, result);
        Assert.Equal(timeout2, session.Object.Timeout);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void IsSynchronized(bool isSynchronized)
    {
        // Arrange
        var session = new Mock<ISessionState>();
        session.Setup(s => s.IsSynchronized).Returns(isSynchronized);

        var state = new HttpSessionState(session.Object);

        // Act
        var result = state.IsSynchronized;

        // Assert
        Assert.Equal(isSynchronized, result);
    }

    [Fact]
    public void SyncRoot()
    {
        // Arrange
        var sync = new object();
        var session = new Mock<ISessionState>();
        session.Setup(s => s.SyncRoot).Returns(sync);

        var state = new HttpSessionState(session.Object);

        // Act
        var result = state.SyncRoot;

        // Assert
        Assert.Equal(sync, result);
    }

    [Fact]
    public void Abandon()
    {
        // Arrange
        var session = new Mock<ISessionState>();
        session.SetupProperty(s => s.IsAbandoned);

        var state = new HttpSessionState(session.Object);

        // Act
        state.Abandon();

        // Assert
        Assert.True(session.Object.IsAbandoned);
    }

    [Fact]
    public void Get()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var value = new object();

        var session = new Mock<ISessionState>();
        session.Setup(s => s[key]).Returns(value);

        var state = new HttpSessionState(session.Object);

        // Act
        var result = state[key];

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void Set()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var value = new object();

        var session = new Mock<ISessionState>();

        var state = new HttpSessionState(session.Object);

        // Act
        state[key] = value;

        // Assert
        session.VerifySet(s => s[key] = value, Times.Once);
    }

    [Fact]
    public void Add()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var value = new object();

        var session = new Mock<ISessionState>();
        var state = new HttpSessionState(session.Object);

        // Act
        state.Add(key, value);

        // Assert
        session.VerifySet(s => s[key] = value, Times.Once);
    }

    [Fact]
    public void Remove()
    {
        // Arrange
        var key = _fixture.Create<string>();

        var session = new Mock<ISessionState>();
        var state = new HttpSessionState(session.Object);

        // Act
        state.Remove(key);

        // Assert
        session.Verify(s => s.Remove(key), Times.Once);
    }

    [Fact]
    public void RemoveAll()
    {
        // Arrange
        var key = _fixture.Create<string>();

        var session = new Mock<ISessionState>();
        var state = new HttpSessionState(session.Object);

        // Act
        state.RemoveAll();

        // Assert
        session.Verify(s => s.Clear(), Times.Once);
    }

    [Fact]
    public void Clear()
    {
        // Arrange
        var key = _fixture.Create<string>();

        var session = new Mock<ISessionState>();
        var state = new HttpSessionState(session.Object);

        // Act
        state.Clear();

        // Assert
        session.Verify(s => s.Clear(), Times.Once);
    }

    [Fact]
    public void GetEnumerator()
    {
        // Arrange
        var keys = _fixture.CreateMany<string>(2).ToArray();

        var session = new Mock<ISessionState>();
        session.Setup(s => s.Keys).Returns(keys);

        var state = new HttpSessionState(session.Object);

        // Act
        var result = state.GetEnumerator();

        // Assert
        Assert.True(result.MoveNext());
        Assert.Equal(keys[0], result.Current);
        Assert.True(result.MoveNext());
        Assert.Equal(keys[1], result.Current);
        Assert.False(result.MoveNext());
    }

    [Fact]
    public void CopyTo()
    {
        // Arrange
        var key1 = _fixture.Create<string>();
        var item1 = new object();

        var key2 = _fixture.Create<string>();
        var item2 = new object();

        var keys = new[] { key1, key2 };

        var session = new Mock<ISessionState>();
        session.Setup(s => s.Keys).Returns(keys);

        session.Setup(s => s[key1]).Returns(item1);
        session.Setup(s => s[key2]).Returns(item2);

        var state = new HttpSessionState(session.Object);
        var array = new object[3];

        // Act
        state.CopyTo(array, 1);

        // Assert
        Assert.Collection(array,
            item => Assert.Null(item),
            item => Assert.Equal(item1, item),
            item => Assert.Equal(item2, item)); ;
    }
}
