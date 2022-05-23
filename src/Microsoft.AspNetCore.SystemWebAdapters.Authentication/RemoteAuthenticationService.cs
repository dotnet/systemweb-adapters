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

public class RemoteAuthenticationService : IAuthenticationService<RemoteAuthenticationResult>
{
    private const string CookieHeaderName = "Cookie";

    private bool _initialized;
    private readonly HttpClient _client;
    private readonly IAuthenticationResultFactory<RemoteAuthenticationResult> _resultFactory;
    private readonly ILogger<RemoteAuthenticationService> _logger;
    private readonly IOptionsMonitor<RemoteAuthenticationOptions> _optionsMonitor;
    private RemoteAuthenticationOptions? _options;

    public RemoteAuthenticationService(
        HttpClient client,
        IAuthenticationResultFactory<RemoteAuthenticationResult> resultFactory,
        IOptionsMonitor<RemoteAuthenticationOptions> options,
        ILogger<RemoteAuthenticationService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _optionsMonitor = options ?? throw new ArgumentNullException(nameof(options));
    }

    // Finish initializing the http client here since the scheme won't be known
    // until the owning authentication handler is initialized.
    public void Initialize(AuthenticationScheme scheme)
    {
        _options = _optionsMonitor.Get(scheme.Name);
        _client.BaseAddress = new Uri(_options.RemoteServiceOptions.RemoteAppUrl, _options.AuthenticationEndpointPath);
        _client.DefaultRequestHeaders.Add(_options.RemoteServiceOptions.ApiKeyHeader, _options.RemoteServiceOptions.ApiKey);
        _initialized = true;
    }

    [MemberNotNullWhen(true, nameof(_options))]
    private bool Initialized => _initialized;

    public async Task<RemoteAuthenticationResult> AuthenticateAsync(HttpRequest originalRequest, CancellationToken cancellationToken)
    {
        if (!Initialized)
        {
            _logger.LogError("Remote authentication handler must be initialized before authenticating");
            throw new InvalidOperationException("Remote authentication handler must be initialized before authenticating");
        }

        var authRequest = new HttpRequestMessage();
        AddHeaders(_options.HeadersToForward, originalRequest, authRequest);
        AddCookies(_options.CookiesToForward, originalRequest, authRequest);

        var response = await _client.SendAsync(authRequest, cancellationToken);
        _logger.LogDebug("Received remote authentication response with status code {StatusCode}", response.StatusCode);

        return await _resultFactory.CreateRemoteAuthenticationResultAsync(response, _options);
    }

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
