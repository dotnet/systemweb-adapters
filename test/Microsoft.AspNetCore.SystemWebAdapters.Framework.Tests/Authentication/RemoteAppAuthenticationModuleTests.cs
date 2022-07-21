using System;
using System.Collections.Specialized;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class RemoteAppAuthenticationModuleTests
{
    private const string GoodKey = "Key1";
    private const string BadKey = "key1";

    [InlineData(GoodKey, "true", null, 0, null, typeof(RemoteAppAuthenticationHttpHandler))]
    [InlineData(GoodKey, "true", "/a", 0, null, typeof(RemoteAppAuthenticationHttpHandler))]
    [InlineData(null, "true", "/a", 400, null, null)]
    [InlineData(null, "true", null, 400, null, null)]
    [InlineData(BadKey, "true", "/a", 400, null, null)]
    [InlineData(BadKey, "true", null, 400, null, null)]
    [InlineData(GoodKey, null, null, 400, null, null)]
    [InlineData(GoodKey, null, "/a", 302, "/a", null)]
    [InlineData(BadKey, null, null, 400, null, null)]
    [InlineData(BadKey, null, "/a", 302, "/a", null)]
    [InlineData(null, null, null, 400, null, null)]
    [InlineData(null, null, "/a", 302, "/a", null)]
    [InlineData(null, null, "", 400, null, null)]
    [InlineData(null, null, "http://a", 400, null, null)]
    [Theory]
    public void VerifyAuthenticationRequestHandling(string apiKey, string authMigrationHeader, string originalPath, int expectedStatusCode, string expectedRedirect, Type expectedHandlerType)
    {
        // Arrange

        // Create module and options to test
        var authOptions = new RemoteAppAuthenticationServerOptions();
        var remoteAppOptions = new RemoteAppOptions
        {
            ApiKey = GoodKey
        };

        var module = new RemoteAppAuthenticationModule(Options.Create(remoteAppOptions), Options.Create(authOptions));

        var headers = new NameValueCollection
        {
            { remoteAppOptions.ApiKeyHeader, apiKey },
            { AuthenticationConstants.MigrationAuthenticateRequestHeaderName, authMigrationHeader }
        };

        var queryStrings = new NameValueCollection
        {
            { AuthenticationConstants.OriginalUrlQueryParamName, originalPath }
        };

        var responseHeaders = new NameValueCollection();

        // Mock request, response, and context
        var request = new Mock<HttpRequestBase>();
        request.Setup(r => r.Headers).Returns(headers);
        request.Setup(r => r.QueryString).Returns(queryStrings);
        request.Setup(r => r.Url).Returns(new Uri("http://localhost:8080"));

        var response = new Mock<HttpResponseBase>();
        response.SetupProperty(r => r.StatusCode);
        response.Setup(r => r.Headers).Returns(responseHeaders);

        var context = new Mock<HttpContextBase>();
        context.Setup(c => c.Response).Returns(response.Object);
        context.Setup(c => c.Request).Returns(request.Object);
        context.SetupProperty(c => c.Handler);

        // Act
        module.MapRemoteAuthenticationHandler(context.Object);

        // Assert
        Assert.Equal(expectedStatusCode, response.Object.StatusCode);
        Assert.Equal(expectedRedirect, responseHeaders["Location"]);

        if (expectedHandlerType is null)
        {
            Assert.Null(context.Object.Handler);
        }
        else
        {
            Assert.IsType(expectedHandlerType, context.Object.Handler);
        }
    }
}
