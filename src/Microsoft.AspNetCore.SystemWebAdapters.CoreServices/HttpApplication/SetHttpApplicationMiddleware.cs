// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class SetHttpApplicationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ObjectPool<HttpApplication> _pool;

    public SetHttpApplicationMiddleware(RequestDelegate next, ObjectPool<HttpApplication> pool)
    {
        _next = next;
        _pool = pool;
    }

    /// <summary>
    /// Initializes the registered HttpApplication to force the Start method to be invoked if present.
    /// </summary>
    public static void InitializeHttpApplication(IServiceProvider services)
    {
        var pool = services.GetRequiredService<ObjectPool<HttpApplication>>();
        pool.Return(pool.Get());
    }

    public async Task InvokeAsync(HttpContextCore context)
    {
        var app = _pool.Get();

        try
        {
            context.Features.Set(app);
            app.Context = context;

            SetRequiredFeatures(context, app);

            context.Features.GetRequired<IHttpResponseBufferingFeature>().EnableBuffering(BufferResponseStreamAttribute.DefaultMemoryThreshold, default);

            await context.Features.GetRequired<IHttpApplicationEventsFeature>().RaiseBeginRequestAsync(context.RequestAborted);

            var endFeature = context.Features.GetRequired<IHttpResponseEndFeature>();

            if (!endFeature.IsEnded)
            {
                await _next(context);
            }

            await endFeature.EndAsync();
        }
        finally
        {
            context.Features.Set<IHttpApplicationEventsFeature>(null);
            context.Features.Set<HttpApplication>(null);
            _pool.Return(app);
        }
    }

    private static void SetRequiredFeatures(HttpContextCore context, HttpApplication application)
    {
        var setNotification = new RequestHttpApplicationEventsFeature(application, context);

        context.Features.Set<INotificationFeature>(setNotification);
        context.Features.Set<IHttpResponseEndFeature>(setNotification);
        context.Features.Set<IHttpApplicationEventsFeature>(setNotification);
    }

    private class RequestHttpApplicationEventsFeature : IHttpApplicationEventsFeature, INotificationFeature, IHttpResponseEndFeature
    {
        private readonly IHttpApplicationEventsFeature _app;
        private readonly HttpContextCore _context;
        private readonly IHttpResponseEndFeature _previous;

        public RequestHttpApplicationEventsFeature(IHttpApplicationEventsFeature app, HttpContextCore context)
        {
            _app = app;
            _context = context;
            _previous = _context.Features.GetRequired<IHttpResponseEndFeature>();
        }

        public RequestNotification CurrentNotification { get; set; }

        public bool IsPostNotification { get; set; }

        public bool IsEnded => _previous.IsEnded;

        async ValueTask IHttpApplicationEventsFeature.RaiseAcquireRequestStateAsync(CancellationToken token)
        {
            if (TrySetNotification(RequestNotification.AcquireRequestState))
            {
                await _app.RaiseAcquireRequestStateAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseAuthenticateRequestAsync(CancellationToken token)
        {
            TrySetNotification(RequestNotification.AuthenticateRequest);
            await _app.RaiseAuthenticateRequestAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseAuthorizeRequestAsync(CancellationToken token)
        {
            if (TrySetNotification(RequestNotification.AuthorizeRequest))
            {
                await _app.RaiseAuthorizeRequestAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseBeginRequestAsync(CancellationToken token)
        {
            if (TrySetNotification(RequestNotification.BeginRequest))
            {
                await _app.RaiseBeginRequestAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseEndRequestAsync(CancellationToken token)
        {
            if (TrySetNotification(RequestNotification.EndRequest))
            {
                await _app.RaiseEndRequestAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseErrorAsync(CancellationToken token)
        {
            if (!IsEnded)
            {
                await _app.RaiseErrorAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseLogRequestAsync(CancellationToken token)
        {
            if (TrySetNotification(RequestNotification.LogRequest))
            {
                await _app.RaiseLogRequestAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseMapRequestHandlerAsync(CancellationToken token)
        {
            if (TrySetNotification(RequestNotification.MapRequestHandler))
            {
                await _app.RaiseMapRequestHandlerAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostAcquireRequestStateAsync(CancellationToken token)
        {
            if (TrySetPostNotification(RequestNotification.AcquireRequestState))
            {
                await _app.RaisePostAcquireRequestStateAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostAuthenticateRequestAsync(CancellationToken token)
        {
            if (TrySetPostNotification(RequestNotification.AuthenticateRequest))
            {
                await _app.RaisePostAuthenticateRequestAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostAuthorizeRequestAsync(CancellationToken token)
        {
            if (TrySetPostNotification(RequestNotification.AuthorizeRequest))
            {
                await _app.RaisePostAuthorizeRequestAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostLogRequestAsync(CancellationToken token)
        {
            if (TrySetPostNotification(RequestNotification.LogRequest))
            {
                await _app.RaisePostLogRequestAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostMapRequestHandlerAsync(CancellationToken token)
        {
            if (TrySetPostNotification(RequestNotification.MapRequestHandler))
            {
                await _app.RaisePostMapRequestHandlerAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostReleaseRequestStateAsync(CancellationToken token)
        {
            if (TrySetPostNotification(RequestNotification.ReleaseRequestState))
            {
                await _app.RaisePostReleaseRequestStateAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostRequestHandlerExecuteAsync(CancellationToken token)
        {
            if (TrySetPostNotification(RequestNotification.ExecuteRequestHandler))
            {
                await _app.RaisePostRequestHandlerExecuteAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostResolveRequestCacheAsync(CancellationToken token)
        {
            if (TrySetPostNotification(RequestNotification.ResolveRequestCache))
            {
                await _app.RaisePostResolveRequestCacheAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostUpdateRequestCacheAsync(CancellationToken token)
        {
            if (TrySetPostNotification(RequestNotification.UpdateRequestCache))
            {
                await _app.RaisePostUpdateRequestCacheAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePreRequestHandlerExecuteAsync(CancellationToken token)
        {
            if (TrySetNotification(RequestNotification.PreExecuteRequestHandler))
            {
                await _app.RaisePreRequestHandlerExecuteAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseReleaseRequestStateAsync(CancellationToken token)
        {
            if (TrySetNotification(RequestNotification.ReleaseRequestState))
            {
                await _app.RaiseReleaseRequestStateAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseRequestCompletedAsync(CancellationToken token)
        {
            if (TrySetNotification(RequestNotification.EndRequest))
            {
                await _app.RaiseRequestCompletedAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseResolveRequestCacheAsync(CancellationToken token)
        {
            if (TrySetNotification(RequestNotification.ResolveRequestCache))
            {
                await _app.RaiseResolveRequestCacheAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseSessionEnd(CancellationToken token)
        {
            if (!IsEnded)
            {
                await _app.RaiseSessionEnd(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseSessionStart(CancellationToken token)
        {
            if (!IsEnded)
            {
                await _app.RaiseSessionStart(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseUpdateRequestCacheAsync(CancellationToken token)
        {
            if (TrySetNotification(RequestNotification.UpdateRequestCache))
            {
                await _app.RaiseUpdateRequestCacheAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePreSendRequestHeaders(CancellationToken token)
        {
            if (!IsEnded)
            {
                await _app.RaisePreSendRequestHeaders(token);
            }
        }

        private bool TrySetPostNotification(RequestNotification notification)
            => TrySetNotification(notification, true);

        private bool TrySetNotification(RequestNotification notification, bool isPost = false)
        {
            CurrentNotification = notification;
            IsPostNotification = isPost;

            return !IsEnded;
        }

        async Task IHttpResponseEndFeature.EndAsync()
        {
            // Prevent subsequent calls to HttpResponse.End() to go through this path and make them no-ops
            _context.Features.Set<IHttpResponseEndFeature>(NonReentrantFeatures.Instance);

            var events = (IHttpApplicationEventsFeature)this;

            await events.RaiseLogRequestAsync(default);
            await events.RaisePostLogRequestAsync(default);
            await events.RaiseEndRequestAsync(default);
            await events.RaisePreSendRequestHeaders(default);

            await _previous.EndAsync();
        }

        private sealed class NonReentrantFeatures : IHttpResponseEndFeature
        {
            public static NonReentrantFeatures Instance { get; } = new();

            public bool IsEnded => true;

            public Task EndAsync() => Task.CompletedTask;
        }
    }
}
