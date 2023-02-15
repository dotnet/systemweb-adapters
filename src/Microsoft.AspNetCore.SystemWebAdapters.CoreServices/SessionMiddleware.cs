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
            : NoSessionAsync(context);

    private Task NoSessionAsync(HttpContextCore context)
        => context.Features.Get<IHttpApplicationFeature>() is { } events ? RaiseEventsNoSessionAsync(context, events) : _next(context);

    private async Task RaiseEventsNoSessionAsync(HttpContextCore context, IHttpApplicationFeature appFeature)
    {
        await appFeature.RaiseEventAsync(ApplicationEvent.AcquireRequestState);
        await appFeature.RaiseEventAsync(ApplicationEvent.PostAcquireRequestState);

        await _next(context);

        await appFeature.RaiseEventAsync(ApplicationEvent.ReleaseRequestState);
        await appFeature.RaiseEventAsync(ApplicationEvent.PostReleaseRequestState);
    }

    private async Task ManageStateAsync(HttpContextCore context, SessionAttribute metadata)
    {
        LogMessage(metadata.Behavior);

        var appFeature = context.Features.Get<IHttpApplicationFeature>();
        var manager = GetSessionManager(context, appFeature);

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

        context.Features.Set(new HttpSessionState(state, manager.Mode));

        try
        {
            await _next(context);

            using var cts = new CancellationTokenSource(CommitTimeout);
            await state.CommitAsync(cts.Token);

            if (state.IsAbandoned)
            {
                if (appFeature is { })
                {
                    await appFeature.RaiseEventAsync(ApplicationEvent.SessionEnd);
                }
            }
        }
        finally
        {
            context.Features.Set<HttpSessionState?>(null);

            if (appFeature is { })
            {
                await appFeature.RaiseEventAsync(ApplicationEvent.ReleaseRequestState);
                await appFeature.RaiseEventAsync(ApplicationEvent.PostReleaseRequestState);
            }
        }
    }

    private static ISessionManager GetSessionManager(HttpContext context, IHttpApplicationFeature? appFeature)
    {
        var manager = context.RequestServices.GetRequiredService<ISessionManager>();

        if (appFeature is null)
        {
            return manager;
        }

        return new EventingSessionManager(manager, appFeature);
    }

    private class EventingSessionManager : ISessionManager
    {
        private readonly ISessionManager _other;
        private readonly IHttpApplicationFeature _appFeature;

        public EventingSessionManager(ISessionManager other, IHttpApplicationFeature appFeature)
        {
            _other = other;
            _appFeature = appFeature;
        }

        public async Task<ISessionState> CreateAsync(HttpContext context, SessionAttribute metadata)
        {
            var result = await _other.CreateAsync(context, metadata);

            if (result.IsNewSession)
            {
                await _appFeature.RaiseEventAsync(ApplicationEvent.SessionStart);
            }

            await _appFeature.RaiseEventAsync(ApplicationEvent.AcquireRequestState);
            await _appFeature.RaiseEventAsync(ApplicationEvent.PostAcquireRequestState);

            return result;
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
