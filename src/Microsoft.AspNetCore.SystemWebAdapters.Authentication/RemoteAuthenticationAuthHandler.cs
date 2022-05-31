// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// Authentication handler that authenticates users by making requests to a remote app
/// for authentication via a remote authentication service.
/// </summary>
internal class RemoteAuthenticationAuthHandler : AuthenticationHandler<RemoteAppAuthenticationOptions>
{
    private readonly IRemoteAuthenticationService _authService;
    private readonly IEnumerable<IRemoteAuthenticationResultProcessor> _resultProcessors;
    private readonly ILogger<RemoteAuthenticationAuthHandler> _logger;
    private RemoteAuthenticationResult? _remoteAuthenticationResult;

    public RemoteAuthenticationAuthHandler(IRemoteAuthenticationService authService,
                                           IEnumerable<IRemoteAuthenticationResultProcessor> resultProcessors,
                                           IOptionsMonitor<RemoteAppAuthenticationOptions> options,
                                           ILoggerFactory loggerFactory,
                                           UrlEncoder encoder,
                                           ISystemClock clock)
        : base(options, loggerFactory, encoder, clock)
    {
        _logger = loggerFactory.CreateLogger<RemoteAuthenticationAuthHandler>();
        _authService = authService;
        _resultProcessors = resultProcessors;
    }

    protected override Task InitializeHandlerAsync() => _authService.InitializeAsync(Scheme);

    private async Task<RemoteAuthenticationResult> GetRemoteAuthenticationResultAsync()
    {
        if (_remoteAuthenticationResult is null)
        {
            // Retrieve the remote authentication result and apply any processors
            _remoteAuthenticationResult = await _authService.AuthenticateAsync(Context.Request, CancellationToken.None);
            foreach (var processor in _resultProcessors)
            {
                await processor.ProcessAsync(_remoteAuthenticationResult, Context);
            }

            if (_remoteAuthenticationResult.StatusCode == 407)
            {
                _logger.LogError("Failed to authenticate using the remote app due to invalid or missing API key");
                throw new InvalidOperationException("Failed to authenticate using the remote app due to invalid or missing API key");
            }
        }

        return _remoteAuthenticationResult;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authResult = await GetRemoteAuthenticationResultAsync();

        if (authResult.User is not null)
        {
            var ticket = new AuthenticationTicket(authResult.User, Scheme.Name);
            _logger.LogDebug("Authenticated user based on remote authentication service");
            return AuthenticateResult.Success(ticket);
        }
        else
        {
            _logger.LogDebug("Remote service did not authenticate a user");
            return AuthenticateResult.NoResult();
        }
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var authResult = await GetRemoteAuthenticationResultAsync();

        // Propagate headers and status code back to the caller
        // Different authentication schemes may challenge in different ways in the remote
        // app, so make a best effort to forward the effects of these challenges by forwarding
        // configured headers (like Location, perhaps) and status code (like 302 or 401, for example).
        Context.Response.StatusCode = authResult.StatusCode;
        foreach (var header in authResult.ResponseHeaders)
        {
            Context.Response.Headers.Append(header.Key, header.Value);
        }
    }
}
