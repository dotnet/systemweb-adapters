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
internal partial class RemoteAppAuthenticationAuthHandler : AuthenticationHandler<RemoteAppAuthenticationClientOptions>
{
    private readonly IRemoteAppAuthenticationService _authService;
    private readonly IEnumerable<IRemoteAppAuthenticationResultProcessor> _resultProcessors;
    private readonly ILogger<RemoteAppAuthenticationAuthHandler> _logger;
    private RemoteAppAuthenticationResult? _remoteAppAuthResult;

    public RemoteAppAuthenticationAuthHandler(IRemoteAppAuthenticationService authService,
                                           IEnumerable<IRemoteAppAuthenticationResultProcessor> resultProcessors,
                                           IOptionsMonitor<RemoteAppAuthenticationClientOptions> options,
                                           ILoggerFactory loggerFactory,
                                           UrlEncoder encoder
        ) : base(options, loggerFactory, encoder)
    {
        _logger = loggerFactory.CreateLogger<RemoteAppAuthenticationAuthHandler>();
        _authService = authService;
        _resultProcessors = resultProcessors;
    }

    private async Task<RemoteAppAuthenticationResult> GetRemoteAppAuthenticationResultAsync()
    {
        if (_remoteAppAuthResult is null)
        {
            // Retrieve the remote authentication result and apply any processors
            _remoteAppAuthResult = await _authService.AuthenticateAsync(Scheme, Context.Request, Context.RequestAborted);
            foreach (var processor in _resultProcessors)
            {
                await processor.ProcessAsync(_remoteAppAuthResult, Context);
            }

            // Different authentication schemes may challenge in different ways in the remote
            // app, so make a best effort to forward the effects of these challenges by forwarding
            // configured headers (like Location, perhaps). Additionally, the act of authentication may
            // reissue an authentication cookie, for example, so headers (i.e. Set-Cookie) will need 
            // to be propagated for authenticate calls too.
            foreach (var header in _remoteAppAuthResult.ResponseHeaders)
            {
                Context.Response.Headers.Append(header.Key, header.Value);
            }

            if (_remoteAppAuthResult.StatusCode == 400)
            {
                LogInvalidApiKey();
                throw new InvalidOperationException("Failed to authenticate using the remote app due to invalid or missing API key");
            }

            Context.Response.OnStarting(static state =>
            {
                if (((HttpContextCore)state).Response.Headers.Location.Count > 1)
                {
                    throw new InvalidOperationException("Multiple Location headers were detected. This is possibly because you have remote authentication as the default scheme. Consider short circuiting the YARP requests or disabling default scheme.");
                }

                return Task.CompletedTask;
            }, Context);
        }

        return _remoteAppAuthResult;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authResult = await GetRemoteAppAuthenticationResultAsync();

        if (authResult.User is not null)
        {
            var ticket = new AuthenticationTicket(authResult.User, Scheme.Name);
            LogUserAuthenticated();
            return AuthenticateResult.Success(ticket);
        }
        else
        {
            LogUserNotAuthenticated();
            return AuthenticateResult.NoResult();
        }
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var authResult = await GetRemoteAppAuthenticationResultAsync();

        // Propagate status code back to the caller to forward the effect of the challenge as best we can.
        Context.Response.StatusCode = authResult.StatusCode;
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Failed to authenticate using the remote app due to invalid or missing API key")]
    private partial void LogInvalidApiKey();

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Authenticated user based on remote authentication service")]
    private partial void LogUserAuthenticated();

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Remote service did not authenticate a user")]
    private partial void LogUserNotAuthenticated();
}
