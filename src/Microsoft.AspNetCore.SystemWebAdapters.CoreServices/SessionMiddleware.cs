// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.SessionState;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters;
internal partial class SessionLoadMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionLoadMiddleware> _logger;

    [LoggerMessage(EventId = 0, Level = LogLevel.Trace, Message = "Initializing session state: {Behavior}")]
    private partial void LogMessage(SessionStateBehavior behavior);

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Creating session on demand by synchronously waiting on a potential asynchronous connection")]
    private partial void LogOnDemand();

    private readonly TimeSpan CommitTimeout = TimeSpan.FromMinutes(1);

    public SessionLoadMiddleware(RequestDelegate next, ILogger<SessionLoadMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContextCore context)
        => context.Features.GetRequired<ISessionStateFeature>() is { Behavior: not SessionStateBehavior.Disabled and not SessionStateBehavior.Default } feature
            ? ManageStateAsync(context, feature)
            : _next(context);

    private async Task ManageStateAsync(HttpContextCore context, ISessionStateFeature feature)
    {
        LogMessage(feature.Behavior);

        var manager = context.RequestServices.GetRequiredService<ISessionManager>();
        var details = new SessionAttribute { SessionBehavior = feature.Behavior, IsLazyLoad = feature.IsLazyLoad };

        using var state = feature.IsLazyLoad
#pragma warning disable CA2000 // False positive for CA2000 here
            ? new LazySessionState(context, LogOnDemand, details, manager)
#pragma warning restore CA2000 // Dispose objects before losing scope
            : await manager.CreateAsync(context, details);

        feature.State = state;

        try
        {
            await _next(context);

            using var cts = new CancellationTokenSource(CommitTimeout);

            if (!details.IsReadOnly)
            {
                await state.CommitAsync(cts.Token);
            }
        }
        finally
        {
            feature.State = null;
        }
    }

    private class LazySessionState : DelegatingSessionState
    {
        private readonly Lazy<ISessionState> _state;

        public LazySessionState(HttpContextCore context, Action log, SessionAttribute metadata, ISessionManager manager)
        {
            _state = new Lazy<ISessionState>(() =>
            {
                log();
                return manager.CreateAsync(context, metadata).GetAwaiter().GetResult();
            });
        }

        protected override ISessionState State => _state.Value;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && _state.IsValueCreated)
            {
                _state.Value.Dispose();
            }
        }
    }
}
