// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using AutoFixture;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession.Tests;

public class GetWriteableSessionHandlerTests
{
    private readonly Fixture _fixture;

    public GetWriteableSessionHandlerTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void SessionInterfaces()
    {
        // Arrange
        var serializer = new Mock<ISessionSerializer>();
        var sessions = new Mock<ILockedSessionCache>();
        object handler = new GetWriteableSessionHandler(serializer.Object, sessions.Object);

        // Assert
        Assert.True(handler is IRequiresSessionState);
        Assert.False(handler is IReadOnlySessionState);
    }

    [Fact]
    public async Task RequestCompleted()
    {
        // Arrange
        var expectedByte = _fixture.Create<byte>();
        var serializer = new Mock<ISessionSerializer>();
        var session = new Mock<HttpSessionStateBase>();

        var sessions = new Mock<ILockedSessionCache>();
        var lockDisposable = new Mock<IDisposable>();
        Action? callback = null;
        sessions.Setup(s => s.Register(session.Object, It.IsAny<Action>()))
            .Callback<HttpSessionStateBase, Action>((_, a) => callback = a)
            .Returns(lockDisposable.Object);

        var handler = new GetWriteableSessionHandler(serializer.Object, sessions.Object);

        var stream = new MemoryStream();
        var response = new Mock<HttpResponseBase>();
        response.SetupAllProperties();
        response.Setup(s => s.OutputStream).Returns(stream);

        var context = new Mock<HttpContextBase>();
        context.Setup(c => c.Response).Returns(response.Object);
        context.Setup(c => c.Session).Returns(session.Object);

        serializer.Setup(s => s.SerializeAsync(session.Object, stream, It.IsAny<CancellationToken>())).Callback(() =>
        {
            stream.WriteByte(expectedByte);
        });

        // Act
        var task = handler.ProcessRequestAsync(context.Object, default);

        Assert.False(task.IsCompleted);
        lockDisposable.Verify(d => d.Dispose(), Times.Never);
        response.Verify(r => r.FlushAsync(), Times.Once);

        callback!();
        await Assert.ThrowsAsync<TaskCanceledException>(() => task);

        // Assert
        Assert.Equal(200, response.Object.StatusCode);
        Assert.Equal("text/event-stream", response.Object.ContentType);

        var bytes = stream.ToArray();

        Assert.Equal(expectedByte, bytes[0]);
        Assert.All(bytes.Skip(1), b => Assert.Equal((int)'\n', b));

        lockDisposable.Verify(d => d.Dispose(), Times.Once);
    }

    [Fact]
    public async Task DisconnectedRequest()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var expectedByte = _fixture.Create<byte>();
        var serializer = new Mock<ISessionSerializer>();
        var session = new Mock<HttpSessionStateBase>();

        var sessions = new Mock<ILockedSessionCache>();
        var lockDisposable = new Mock<IDisposable>();
        sessions.Setup(s => s.Register(session.Object, It.IsAny<Action>())).Returns(lockDisposable.Object);

        var handler = new GetWriteableSessionHandler(serializer.Object, sessions.Object);


        var stream = new MemoryStream();
        var response = new Mock<HttpResponseBase>();
        response.SetupAllProperties();
        response.Setup(s => s.OutputStream).Returns(stream);

        var context = new Mock<HttpContextBase>();
        context.Setup(c => c.Response).Returns(response.Object);
        context.Setup(c => c.Session).Returns(session.Object);

        serializer.Setup(s => s.SerializeAsync(session.Object, stream, It.IsAny<CancellationToken>())).Callback(() =>
        {
            stream.WriteByte(expectedByte);
        });

        // Act
        var task = handler.ProcessRequestAsync(context.Object, cts.Token);

        Assert.False(task.IsCompleted);
        lockDisposable.Verify(d => d.Dispose(), Times.Never);
        response.Verify(r => r.FlushAsync(), Times.Once);

        cts.Cancel();
        await Assert.ThrowsAsync<TaskCanceledException>(() => task);

        // Assert
        Assert.Equal(200, response.Object.StatusCode);
        Assert.Equal("text/event-stream", response.Object.ContentType);

        var bytes = stream.ToArray();

        Assert.Equal(expectedByte, bytes[0]);
        Assert.All(bytes.Skip(1), b => Assert.Equal((int)'\n', b));

        lockDisposable.Verify(d => d.Dispose(), Times.Once);
    }
}
