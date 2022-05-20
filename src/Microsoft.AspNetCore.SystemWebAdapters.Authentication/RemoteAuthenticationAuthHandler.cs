// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

internal class RemoteAuthenticationAuthHandler : AuthenticationHandler<RemoteAuthenticationOptions>
{
    private readonly IAuthenticationService<RemoteAuthenticationResult> _authService;
    private readonly IEnumerable<IRemoteAuthenticateResultProcessor> _resultProcessors;
    private readonly ILogger<RemoteAuthenticationAuthHandler> _logger;
    private RemoteAuthenticationResult? _remoteAuthenticationResult;

    public RemoteAuthenticationAuthHandler(IAuthenticationService<RemoteAuthenticationResult> authService,
                                           IEnumerable<IRemoteAuthenticateResultProcessor> resultProcessors,
                                           IOptionsMonitor<RemoteAuthenticationOptions> options,
                                           ILoggerFactory loggerFactory,
                                           UrlEncoder encoder,
                                           ISystemClock clock)
        : base(options, loggerFactory, encoder, clock)
    {
        _logger = loggerFactory.CreateLogger<RemoteAuthenticationAuthHandler>();
        _authService = authService;
        _resultProcessors = resultProcessors;
    }

    protected override Task InitializeHandlerAsync()
    {
        _authService.Initialize(Scheme);
        return Task.CompletedTask;
    }

    private async Task<RemoteAuthenticationResult> GetRemoteAuthenticationResultAsync()
    {
        if (_remoteAuthenticationResult is null)
        {
            _remoteAuthenticationResult = await _authService.AuthenticateAsync(Context.Request, CancellationToken.None).ConfigureAwait(false);
            foreach (var processor in _resultProcessors)
            {
                await processor.ProcessAsync(_remoteAuthenticationResult, Context);
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
            return AuthenticateResult.Success(ticket);
        }
        else
        {
            return AuthenticateResult.NoResult();
        }
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var authResult = await GetRemoteAuthenticationResultAsync();

        // Propagate headers and status code back to the caller
        Context.Response.StatusCode = authResult.StatusCode;
        foreach (var header in authResult.ResponseHeaders.Keys)
        {
            Context.Response.Headers.Add(header, authResult.ResponseHeaders[header].ToArray());
        }
    }
}
