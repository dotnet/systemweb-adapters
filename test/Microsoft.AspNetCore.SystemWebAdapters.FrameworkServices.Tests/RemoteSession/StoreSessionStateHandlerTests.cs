// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using AutoFixture;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession.Tests;

public class StoreSessionStateHandlerTests
{
    private readonly Fixture _fixture;

    public StoreSessionStateHandlerTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void NoSessionStateInterfaces()
    {
        // Arrange
        var cookieName = _fixture.Create<string>();
        var sessions = new Mock<ILockedSessionCache>();
        object handler = new StoreSessionStateHandler(sessions.Object, cookieName);

        // Assert
        Assert.False(handler is IRequiresSessionState);
        Assert.False(handler is IReadOnlySessionState);
    }

    [Fact]
    public async Task NoCookieSet()
    {
        // Arrange
        var cookies = new HttpCookieCollection();
        var cookieName = _fixture.Create<string>();
        var sessions = new Mock<ILockedSessionCache>();
        var handler = new StoreSessionStateHandler(sessions.Object, cookieName);

        var response = new Mock<HttpResponseBase>();
        response.SetupProperty(r => r.StatusCode);
        response.SetupProperty(r => r.StatusDescription);

        var request = new Mock<HttpRequestBase>();
        request.Setup(r => r.Cookies).Returns(cookies);

        var context = new Mock<HttpContextBase>();
        context.Setup(c => c.Response).Returns(response.Object);
        context.Setup(c => c.Request).Returns(request.Object);

        // Act
        await handler.ProcessRequestAsync(context.Object);

        // Assert
        Assert.Equal(400, response.Object.StatusCode);
        Assert.Equal(StoreSessionStateHandler.Messages.NoSessionId, response.Object.StatusDescription);
    }

    [InlineData((int)SessionSaveResult.Success, 200, null)]
    [InlineData((int)SessionSaveResult.SessionNotFound, 400, StoreSessionStateHandler.Messages.SessionNotFound)]
    [InlineData((int)SessionSaveResult.AlreadyUpdated, 400, StoreSessionStateHandler.Messages.SessionAlreadyUpdated)]
    [InlineData((int)SessionSaveResult.DeserializationError, 400, StoreSessionStateHandler.Messages.DeserializationFailed)]
    [Theory]
    public async Task WithCookie(int result, int statusCode, string? description)
    {
        // Arrange
        var output = new Mock<Stream>();

        var cookies = new HttpCookieCollection();
        var cookieName = _fixture.Create<string>();
        var sessionId = _fixture.Create<string>();
        cookies.Set(new(cookieName, sessionId));

        var sessions = new Mock<ILockedSessionCache>();
        sessions.Setup(s => s.SaveAsync(sessionId, output.Object, default)).ReturnsAsync((SessionSaveResult)result);

        var response = new Mock<HttpResponseBase>();
        response.SetupProperty(r => r.StatusCode);
        response.SetupProperty(r => r.StatusDescription);

        var request = new Mock<HttpRequestBase>();
        request.Setup(r => r.GetBufferlessInputStream()).Returns(output.Object);
        request.Setup(r => r.Cookies).Returns(cookies);

        var context = new Mock<HttpContextBase>();
        context.Setup(c => c.Response).Returns(response.Object);
        context.Setup(c => c.Request).Returns(request.Object);

        var handler = new StoreSessionStateHandler(sessions.Object, cookieName);

        // Act
        await handler.ProcessRequestAsync(context.Object);

        // Assert
        Assert.Equal(statusCode, response.Object.StatusCode);
        Assert.Equal(description, response.Object.StatusDescription);

        sessions.Verify(s => s.SaveAsync(sessionId, output.Object, default), Times.Once);
    }
}
