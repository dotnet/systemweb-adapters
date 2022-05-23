// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession.Tests;

public class ReadOnlySessionHandlerTests
{
    [Fact]
    public void ImplementsReadOnlyState()
    {
        // Arrange
        var serializer = new Mock<ISessionSerializer>();
        var handler = new ReadOnlySessionHandler(serializer.Object);

        // Assert
        Assert.IsAssignableFrom<IReadOnlySessionState>(handler);
    }

    [Fact]
    public async Task Process()
    {

        // Arrange
        var serializer = new Mock<ISessionSerializer>();
        var handler = new ReadOnlySessionHandler(serializer.Object);

        var output = new Mock<Stream>();
        var session = new Mock<HttpSessionStateBase>();

        var response = new Mock<HttpResponseBase>();
        response.SetupProperty(r => r.ContentType);
        response.SetupProperty(r => r.StatusCode);
        response.Setup(r => r.OutputStream).Returns(output.Object);

        var context = new Mock<HttpContextBase>();
        context.Setup(c => c.Response).Returns(response.Object);
        context.Setup(c => c.Session).Returns(session.Object);

        // Act
        await handler.ProcessRequestAsync(context.Object);

        // Assert
        Assert.Equal(200, response.Object.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.Object.ContentType);

        serializer.Verify(s => s.SerializeAsync(session.Object, output.Object, default), Times.Once);
    }
}
