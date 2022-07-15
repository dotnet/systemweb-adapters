using System;
using System.Collections.Specialized;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class RemoteAppAuthenticationModuleTests
{
    private const string GoodKey = "Key1";
    private const string BadKey = "key1";

    [Fact]
    public void VerifyOptionsIsNotNull()
    {
        Assert.Throws<ArgumentNullException>(() => new RemoteAppAuthenticationModule(null!));
    }

    [InlineData("AuthEndpoint", "HeaderName", "MyKey", true)]
    [InlineData(null, "HeaderName", "MyKey", false)]
    [InlineData("", "HeaderName", "MyKey", false)]
    [InlineData("AuthEndpoint", null, "MyKey", false)]
    [InlineData("AuthEndpoint", "", "MyKey", false)]
    [InlineData("AuthEndpoint", "HeaderName", null, false)]
    [InlineData("AuthEndpoint", "HeaderName", "", false)]
    [Theory]
    public void VerifyOptionsMembersAreNotNullOrEmpty(string authEndpoint, string apiKeyHeader, string apiKey, bool shouldSucceed)
    {
        var options = new RemoteAppAuthenticationOptions
        {
            AuthenticationEndpointPath = authEndpoint,
            RemoteServiceOptions = new RemoteServiceOptions
            {
                ApiKeyHeader = apiKeyHeader,
                ApiKey = apiKey
            }
        };

        if (shouldSucceed)
        {
            _ = new RemoteAppAuthenticationModule(options);
        }
        else
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RemoteAppAuthenticationModule(options));
        }
    }

    [InlineData(GoodKey, 0, typeof(RemoteAppAuthenticationHttpHandler))]
    [InlineData(BadKey, 400, null)]
    [InlineData(null, 400, null)]
    [Theory]
    public void VerifyAuthenticationRequestHandling(string apiKey, int expectedStatusCode, Type expectedHandlerType)
    {
        // Arrange

        // Create module and options to test
        var options = new RemoteAppAuthenticationOptions();
        options.RemoteServiceOptions.ApiKey = GoodKey;

        var module = new RemoteAppAuthenticationModule(options);

        var headers = new NameValueCollection
        {
            { options.RemoteServiceOptions.ApiKeyHeader, apiKey }
        };

        var responseHeaders = new NameValueCollection();

        // Mock request, response, and context
        var request = new Mock<HttpRequestBase>();
        request.Setup(r => r.Headers).Returns(headers);

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
        Assert.Null(responseHeaders["Location"]);

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
