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

    [InlineData("original url", GoodKey, 0, null, typeof(RemoteAppAuthenticationHttpHandler))]
    [InlineData("original url", BadKey, 407, null, null)]
    [InlineData(null, null, 400, null, null)]
    [InlineData("original url", null, 302, "original url", null)]
    [Theory]
    public void VerifyAuthenticationRequestHandling(string queryParamValue, string apiKey, int expectedStatusCode, string expectedRedirect, Type expectedHandlerType)
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

        var queryStrings = new NameValueCollection
        {
            { AuthenticationConstants.OriginalUrlQueryParamName, queryParamValue }
        };

        var responseHeaders = new NameValueCollection();

        // Mock request, response, and context
        var request = new Mock<HttpRequestBase>();
        request.Setup(r => r.Headers).Returns(headers);
        request.Setup(r => r.QueryString).Returns(queryStrings);

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
