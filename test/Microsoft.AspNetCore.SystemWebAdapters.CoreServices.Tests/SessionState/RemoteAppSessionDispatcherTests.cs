// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession.Tests;

public class RemoteAppSessionDispatcherTests
{
    [Theory]
    [InlineData(true, SessionStateBehavior.ReadOnly, false)]
    [InlineData(true, SessionStateBehavior.Required, true)]
    [InlineData(false, SessionStateBehavior.ReadOnly, false)]
    [InlineData(false, SessionStateBehavior.Required, false)]
    public async Task DispatcherTest(bool useSingleConnection, SessionStateBehavior behavior, bool isSingleExpected)
    {
        // Arrange
        var options = new RemoteAppSessionStateClientOptions()
        {
            UseSingleConnection = useSingleConnection,
        };

        var context = new DefaultHttpContext();

        var single = new Mock<ISessionManager>();
        var singleState = new Mock<ISessionState>();
        single.Setup(s => s.CreateAsync(It.IsAny<HttpContextCore>(), It.IsAny<SessionAttribute>())).ReturnsAsync(singleState.Object);

        var @double = new Mock<ISessionManager>();
        var doubleState = new Mock<ISessionState>();
        @double.Setup(s => s.CreateAsync(It.IsAny<HttpContextCore>(), It.IsAny<SessionAttribute>())).ReturnsAsync(doubleState.Object);

        var optionsProvider = new Mock<IOptions<RemoteAppSessionStateClientOptions>>();
        optionsProvider.Setup(o => o.Value).Returns(options);

        var s = RemoteAppSessionDispatcher.Create(optionsProvider.Object, single.Object, @double.Object);
        var expected = isSingleExpected ? singleState.Object : doubleState.Object;

        // Act
        using var state = await s.CreateAsync(context, new SessionAttribute { SessionBehavior = behavior });

        // Assert

        Assert.Same(expected, state);
    }

    [Fact]
    public Task HandleServerDoesNotSupportSingleConnection()
        => RunError(() => throw new HttpRequestException(null, null, HttpStatusCode.MethodNotAllowed));

#if NET8_0_OR_GREATER
    [Fact]
    public Task HandleHttpProtocolError()
        => RunError(() => throw new HttpRequestException(HttpRequestError.HttpProtocolError));
#endif

    private static async Task RunError(Action action)
    {
        // Arrange
        var options = new RemoteAppSessionStateClientOptions()
        {
            UseSingleConnection = true,
        };

        var context = new DefaultHttpContext();

        var single = new Mock<ISessionManager>();
        var singleState = new Mock<ISessionState>();
        single.Setup(s => s.CreateAsync(It.IsAny<HttpContextCore>(), It.IsAny<SessionAttribute>())).Callback(action);

        var @double = new Mock<ISessionManager>();
        var doubleState = new Mock<ISessionState>();
        @double.Setup(s => s.CreateAsync(It.IsAny<HttpContextCore>(), It.IsAny<SessionAttribute>())).ReturnsAsync(doubleState.Object);

        var optionsProvider = new Mock<IOptions<RemoteAppSessionStateClientOptions>>();
        optionsProvider.Setup(o => o.Value).Returns(options);

        var s = RemoteAppSessionDispatcher.Create(optionsProvider.Object, single.Object, @double.Object);

        // Act
        using var state = await s.CreateAsync(context, new SessionAttribute { SessionBehavior = SessionStateBehavior.Required });

        // Assert
        Assert.Same(doubleState.Object, state);
    }
}

