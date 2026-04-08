// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Specialized;
using System.Web;
using AutoFixture;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession.Tests;

public class RemoteSessionModuleTests
{
    private const string ApiKey1 = "some-key";
    private const string ApiKey2 = "some-Key";

    private readonly Fixture _fixture;

    public RemoteSessionModuleTests()
    {
        _fixture = new Fixture();
    }

    [InlineData("GET", "true", 401, ApiKey1, ApiKey2, null)]
    [InlineData("GET", "true", 0, ApiKey1, ApiKey1, typeof(ReadOnlySessionHandler))]
    [InlineData("GET", "false", 0, ApiKey1, ApiKey1, typeof(GetWriteableSessionHandler))]
    [InlineData("GET", "false", 401, ApiKey1, ApiKey2, null)]
    [InlineData("GET", "", 0, ApiKey1, ApiKey1, typeof(GetWriteableSessionHandler))]
    [InlineData("GET", "", 401, ApiKey1, ApiKey2, null)]
    [InlineData("GET", "non-boolean", 0, ApiKey1, ApiKey1, typeof(GetWriteableSessionHandler))]
    [InlineData("GET", null, 0, ApiKey1, ApiKey1, typeof(GetWriteableSessionHandler))]
    [InlineData("PUT", null, 0, ApiKey1, ApiKey1, typeof(StoreSessionStateHandler))]
    [InlineData("PUT", "true", 0, ApiKey1, ApiKey1, typeof(StoreSessionStateHandler))]
    [InlineData("PUT", "false", 0, ApiKey1, ApiKey1, typeof(StoreSessionStateHandler))]
    [InlineData("POST", null, 0, ApiKey1, ApiKey1, typeof(ReadWriteSessionHandler))]
    [InlineData("Post", "true", 0, ApiKey1, ApiKey1, typeof(ReadWriteSessionHandler))]
    [InlineData("Post", "false", 0, ApiKey1, ApiKey1, typeof(ReadWriteSessionHandler))]
    [Theory]
    public void VerifyCorrectHandler(string method, string? readOnlyHeaderValue, int statusCode, string expectedApiKey, string apiKey, Type? handlerType)
    {
        // Arrange
        var sessionOptions = Options.Create(new RemoteAppSessionStateServerOptions());
        var remoteAppOptions = Options.Create(new RemoteAppServerOptions { ApiKey = expectedApiKey });

        var sessions = new Mock<ILockedSessionCache>();
        var serializer = new Mock<ISessionSerializer>();

        var module = new RemoteSessionModule(sessionOptions, remoteAppOptions, sessions.Object, serializer.Object);

        var headers = new NameValueCollection
        {
            { remoteAppOptions.Value.ApiKeyHeader, apiKey },
            { SessionConstants.ReadOnlyHeaderName, readOnlyHeaderValue }
        };

        var response = new Mock<HttpResponseBase>();
        response.SetupProperty(t => t.StatusCode);

        var request = new Mock<HttpRequestBase>();
        request.Setup(r => r.Headers).Returns(headers);
        request.Setup(r => r.HttpMethod).Returns(method);

        var context = new Mock<HttpContextBase>();
        context.Setup(c => c.Response).Returns(response.Object);
        context.Setup(c => c.Request).Returns(request.Object);
        context.SetupProperty(c => c.Handler);

        // Act
        module.HandleRequest(context.Object);

        // Assert
        Assert.Equal(statusCode, response.Object.StatusCode);

        if (handlerType is null)
        {
            Assert.Null(context.Object.Handler);
        }
        else
        {
            Assert.IsType(handlerType, context.Object.Handler);
        }
    }
}
