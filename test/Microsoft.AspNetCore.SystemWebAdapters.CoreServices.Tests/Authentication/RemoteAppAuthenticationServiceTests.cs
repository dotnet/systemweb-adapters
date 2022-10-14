using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication.Tests;

public class RemoteAppAuthenticationServiceTests
{
    [Theory]
    [MemberData(nameof(GetHeadersForConcatenation))]
    public void AddHeadersConcatenation(Dictionary<string, StringValues> originalHeaders, Dictionary<string, StringValues> resultHeaders)
    {
        var originalRequest = new Mock<HttpRequestCore>();
        originalRequest.Setup(r => r.Headers).Returns(new HeaderDictionary(originalHeaders));
        originalRequest.Setup(r => r.Scheme).Returns("scheme");
        originalRequest.Setup(r => r.Host).Returns(new HostString("host"));

        // Add additional headers that are added to the original request's headers
        resultHeaders.Add(AuthenticationConstants.ForwardedHostHeaderName, "host");
        resultHeaders.Add(AuthenticationConstants.ForwardedProtoHeaderName, "scheme");
        resultHeaders.Add(AuthenticationConstants.MigrationAuthenticateRequestHeaderName, "true");

        var authRequest = new HttpRequestMessage();

        RemoteAppAuthenticationService.AddHeaders(originalHeaders.Keys, originalRequest.Object, authRequest);

        Assert.Collection(
            authRequest.Headers.OrderBy(h => h.Key).Select(h => KeyValuePair.Create(h.Key, new StringValues(h.Value.ToArray()))),
            resultHeaders.OrderBy(h => h.Key).Select<KeyValuePair<string, StringValues>, Action<KeyValuePair<string, StringValues>>>(
                expected => (actual =>
                {
                    Assert.Equal(expected.Key, actual.Key);
                    Assert.Equal(expected.Value, actual.Value);
                })).ToArray());
    }

    public static IEnumerable<object[]> GetHeadersForConcatenation()
    {
        // Trivial positive case
        yield return new object[]
        {
            new Dictionary<string, StringValues>(){ { "A", new StringValues("1") } },
            new Dictionary<string, StringValues>(){ { "A", new StringValues("1") } }
        };

        // Multi-header case
        yield return new object[]
        {
            new Dictionary<string, StringValues>(){ { "A", new StringValues(new[] { "1", "2" }) } },
            new Dictionary<string, StringValues>(){ { "A", new StringValues(new[] { "1", "2" }) } }
        };

        // Multi-cookie case
        yield return new object[]
        {
            new Dictionary<string, StringValues>(){ { "Cookie", new StringValues(new[] { "1", "2" }) } },
            new Dictionary<string, StringValues>(){ { "Cookie", new StringValues(new[] { "1; 2" }) } }
        };
    }
}
