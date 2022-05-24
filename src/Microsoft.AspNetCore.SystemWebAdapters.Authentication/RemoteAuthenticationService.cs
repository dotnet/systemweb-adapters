// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// Service for authenticating a user by making an HTTP request on their behalf to a remote app.
/// </summary>
internal class RemoteAuthenticationService : IRemoteAuthenticationService
{
    private const string CookieHeaderName = "Cookie";

    private bool _initialized;
    private readonly HttpClient _client;
    private readonly IAuthenticationResultFactory _resultFactory;
    private readonly ILogger<RemoteAuthenticationService> _logger;
    private readonly IOptionsMonitor<RemoteAuthenticationOptions> _optionsMonitor;
    private RemoteAuthenticationOptions? _options;

    public RemoteAuthenticationService(
        HttpClient client,
        IAuthenticationResultFactory resultFactory,
        IOptionsMonitor<RemoteAuthenticationOptions> options,
        ILogger<RemoteAuthenticationService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _optionsMonitor = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Initialize the remote authentication service for a given authenticaiton scheme.
    /// </summary>
    /// <param name="scheme">The scheme whose configuration should be used to authenticate.</param>
    public void Initialize(AuthenticationScheme scheme)
    {
        // Finish initializing the http client here since the scheme won't be known
        // until the owning authentication handler is initialized.
        _options = _optionsMonitor.Get(scheme.Name);
        _client.BaseAddress = new Uri(_options.RemoteServiceOptions.RemoteAppUrl, _options.AuthenticationEndpointPath);
        _client.DefaultRequestHeaders.Add(_options.RemoteServiceOptions.ApiKeyHeader, _options.RemoteServiceOptions.ApiKey);
        _initialized = true;
    }

    [MemberNotNullWhen(true, nameof(_options))]
    private bool Initialized => _initialized;

    /// <summary>
    /// Authenticate the user of a request by making a request to a remote app using configured
    /// headers and/or cookies from the current request.
    /// </summary>
    /// <param name="originalRequest">The HTTP request to authenticate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// An authenticaiton result including the claims principal of the authenticated user (if any)
    /// and a status code and headers to include in the response to the request in case the user could not be authenticated.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// An invalid operation exception is thrown if
    /// AuthenticateAsync is called without calling Initialize.
    /// </exception>
    public async Task<RemoteAuthenticationResult> AuthenticateAsync(HttpRequest originalRequest, CancellationToken cancellationToken)
    {
        if (!Initialized)
        {
            _logger.LogError("Remote authentication handler must be initialized before authenticating");
            throw new InvalidOperationException("Remote authentication handler must be initialized before authenticating");
        }

        // Create a new HTTP request, but propagate along configured headers or cookies
        // that may matter for authentication
        using var authRequest = new HttpRequestMessage();
        AddHeaders(_options.HeadersToForward, originalRequest, authRequest);
        AddCookies(_options.CookiesToForward, originalRequest, authRequest);

        // Get the response from the remote app and convert the response into a remote authentication result
        using var response = await _client.SendAsync(authRequest, cancellationToken);
        _logger.LogDebug("Received remote authentication response with status code {StatusCode}", response.StatusCode);

        return await _resultFactory.CreateRemoteAuthenticationResultAsync(response, _options);
    }

    // Add configured headers to the request, or all headers if none in particualr are specified
    private static void AddHeaders(IEnumerable<string> headersToForward, HttpRequest originalRequest, HttpRequestMessage authRequest)
    {
        IEnumerable<string> headerNames = originalRequest.Headers.Keys;
        if (headersToForward.Any())
        {
            headerNames = headerNames.Where(headersToForward.Contains);
        }

        foreach (var headerName in headerNames)
        {
            authRequest.Headers.Add(headerName, originalRequest.Headers[headerName].ToArray());
        }
    }

    // Add configured cookies to the request, or all cookies if none in particualr are specified
    private static void AddCookies(IEnumerable<string> cookiesToForward, HttpRequest originalRequest, HttpRequestMessage authRequest)
    {
        IEnumerable<string> cookieNames = originalRequest.Cookies.Keys;
        if (cookiesToForward.Any())
        {
            cookieNames = cookieNames.Where(cookiesToForward.Contains);
        }

        var cookies = new List<string>();
        foreach (var cookieName in cookieNames)
        {
            cookies.Add($"{cookieName}={originalRequest.Cookies[cookieName]}");
        }

        if (cookies.Any())
        {
            authRequest.Headers.Add(CookieHeaderName, string.Join("; ", cookies));
        }
    }
}
