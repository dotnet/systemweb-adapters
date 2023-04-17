// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.SessionState;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters;
internal partial class SessionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionMiddleware> _logger;

    [LoggerMessage(EventId = 0, Level = LogLevel.Trace, Message = "Initializing session state: {Behavior}")]
    private partial void LogMessage(SessionBehavior behavior);

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Creating session on demand by synchronously waiting on a potential asynchronous connection")]
    private partial void LogOnDemand();

    private readonly TimeSpan CommitTimeout = TimeSpan.FromMinutes(1);

    public SessionMiddleware(RequestDelegate next, ILogger<SessionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContextCore context)
        => context.GetEndpoint()?.Metadata.GetMetadata<SessionAttribute>() is { Behavior: not SessionBehavior.None } metadata
            ? ManageStateAsync(context, metadata)
            : _next(context);

    private async Task ManageStateAsync(HttpContextCore context, SessionAttribute metadata)
    {
        LogMessage(metadata.Behavior);

        var manager = context.RequestServices.GetRequiredService<ISessionManager>();

        using var state = metadata.Behavior switch
        {
#pragma warning disable CA2000 // False positive for CA2000 here
#pragma warning disable CS0618 // Type or member is obsolete
            SessionBehavior.OnDemand => new LazySessionState(context, LogOnDemand, metadata, manager),
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CA2000 // Dispose objects before losing scope

            SessionBehavior.Preload => await manager.CreateAsync(context, metadata),
            var behavior => throw new InvalidOperationException($"Unknown session behavior {behavior}"),
        };

        context.Features.Set(new HttpSessionState(state));

        try
        {
            await _next(context);

            using var cts = new CancellationTokenSource(CommitTimeout);
            await state.CommitAsync(cts.Token);
        }
        finally
        {
            context.Features.Set<HttpSessionState?>(null);
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
