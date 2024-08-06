// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// Service for authenticating a user by making an HTTP request on their behalf to a remote app.
/// </summary>
internal partial class RemoteAppAuthenticationService : IRemoteAppAuthenticationService
{
    private readonly HttpClient _client;
    private readonly IAuthenticationResultFactory _resultFactory;
    private readonly ILogger<RemoteAppAuthenticationService> _logger;
    private readonly IOptionsMonitor<RemoteAppAuthenticationClientOptions> _authOptionsMonitor;

    public RemoteAppAuthenticationService(
        IAuthenticationResultFactory resultFactory,
        IOptionsMonitor<RemoteAppAuthenticationClientOptions> authOptions,
        IOptions<RemoteAppClientOptions> remoteAppOptions,
        ILogger<RemoteAppAuthenticationService> logger)
    {
        _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authOptionsMonitor = authOptions ?? throw new ArgumentNullException(nameof(authOptions));
        _client = remoteAppOptions?.Value.BackchannelClient ?? throw new ArgumentNullException(nameof(remoteAppOptions));
    }

    /// <summary>
    /// Authenticate the user of a request by making a request to a remote app using configured
    /// headers and/or cookies from the current request.
    /// </summary>
    /// <param name="scheme">The authentication scheme currently in use.</param>
    /// <param name="request">The HTTP request to authenticate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// An authentication result including the claims principal of the authenticated user (if any)
    /// and a status code and headers to include in the response to the request in case the user could not be authenticated.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// An invalid operation exception is thrown if
    /// AuthenticateAsync is called without calling Initialize.
    /// </exception>
    public async Task<RemoteAppAuthenticationResult> AuthenticateAsync(AuthenticationScheme scheme, HttpRequest request, CancellationToken cancellationToken)
    {
        var options = _authOptionsMonitor.Get(scheme.Name);

        // Create a new HTTP request, but propagate along configured headers or cookies
        // that may matter for authentication. Also include the original request path as
        // as a query parameter so that the ASP.NET app can redirect back to it if an
        // authentication provider attempts to redirect back to the authenticate URL.
        var url = $"{options.Path.Relative}?{AuthenticationConstants.OriginalUrlQueryParamName}={WebUtility.UrlEncode(request.GetEncodedPathAndQuery())}";
        using var authRequest = new HttpRequestMessage(HttpMethod.Get, url);
        AddHeaders(options.RequestHeadersToForward, request, authRequest);

        // Get the response from the remote app and convert the response into a remote authentication result
        using var response = await _client.SendAsync(authRequest, cancellationToken);
        LogAuthenticationResponse(response.StatusCode);

        return await _resultFactory.CreateRemoteAppAuthenticationResultAsync(response, options);
    }

    // Add configured headers to the request, or all headers if none in particular are specified
    internal static void AddHeaders(IEnumerable<string> headersToForward, HttpRequest originalRequest, HttpRequestMessage authRequest)
    {
        // Add x-forwarded headers so that the authenticate API will know which host the HTTP request was addressed to originally.
        // These headers are also used by result processors - to fix-up redirect responses, for example, to redirect back to the
        // correct host.
        authRequest.Headers.Add(AuthenticationConstants.ForwardedHostHeaderName, originalRequest.Host.Value);
        authRequest.Headers.Add(AuthenticationConstants.ForwardedProtoHeaderName, originalRequest.Scheme);

        // The migration authentication request header indicates that the request is from the ASP.NET Core app
        // with the intention of authenticating the user. Without this header, the request will be interpreted
        // as a callback after authenticating with an identity provider.
        authRequest.Headers.Add(AuthenticationConstants.MigrationAuthenticateRequestHeaderName, "true");

        IEnumerable<string> headerNames = originalRequest.Headers.Keys;
        if (headersToForward.Any())
        {
            headerNames = headerNames.Where(headersToForward.Contains);
        }

        foreach (var headerName in headerNames)
        {
            var originalHeaders = (IEnumerable<string>)originalRequest.Headers[headerName];

            // Workaround for an issue identified by https://github.com/dotnet/systemweb-adapters/issues/228.
            // HttpClient wrongly uses comma (",") instead of semi-colon (";") as a separator for Cookie headers.
            // To mitigate this, we concatenate them manually and put them back as a single header value.
            // This workaround can be removed once we target .NET 7+ as Kestrel is fixed there.
            if (string.Equals(headerName, HeaderNames.Cookie, StringComparison.OrdinalIgnoreCase))
            {
                authRequest.Headers.Add(headerName, string.Join("; ", originalHeaders));
                continue;
            }

            // Workaround for an issue when adding an empty Authorization header
            if (string.Equals(headerName, HeaderNames.Authorization, StringComparison.OrdinalIgnoreCase))
            {
                originalHeaders = originalHeaders.Where(x => !string.IsNullOrEmpty(x));
            }

            authRequest.Headers.Add(headerName, originalHeaders);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Received remote authentication response with status code {StatusCode}")]
    private partial void LogAuthenticationResponse(HttpStatusCode statusCode);
}
