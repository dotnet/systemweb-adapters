// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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

    public async Task InvokeAsync(HttpContextCore context)
    {
        var app = _pool.Get();

        try
        {
            context.Features.Set(app);
            app.Context = context;

            var feature = SetRequiredFeatures(context, app);

            await feature.RaiseBeginRequestAsync(context.RequestAborted);
            await _next(context);
            await feature.RaiseEndRequestAsync(context.RequestAborted);
        }
        finally
        {
            context.Features.Set<HttpApplication>(null);
            _pool.Return(app);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "Fixed by https://github.com/dotnet/roslyn-analyzers/pull/6418")]
    private static IHttpApplicationEventsFeature SetRequiredFeatures(HttpContextCore context, HttpApplication application)
    {
        context.Response.OnStarting(async static state =>
        {
            var context = (HttpContextCore)state;

            if (context.Features.Get<IHttpApplicationEventsFeature>() is { } feature)
            {
                await feature.RaisePreSendRequestHeaders(context.RequestAborted);
            }
        }, context);

        context.Response.OnCompleted(async static state =>
        {
            var context = (HttpContextCore)state;

            if (context.Features.Get<IHttpApplicationEventsFeature>() is { } feature)
            {
                await feature.RaiseRequestCompletedAsync(context.RequestAborted);
            }
        }, context);

        var setNotification = new RequestHttpApplicationEventsFeature(application, context);

        if (context.Features.Get<IHttpApplicationEventsFeature>() is { } existingEventFeature)
        {
            context.Features.Set<IHttpApplicationEventsFeature>(new CompositeHttpApplicationEventsFeature(existingEventFeature, setNotification));
        }
        else
        {
            context.Features.Set<IHttpApplicationEventsFeature>(setNotification);
        }

        context.Features.Set<INotificationFeature>(setNotification);

        // Need to support Response.IsEnded potentially before the buffering support is added
        context.Features.Set<IHttpResponseAdapterFeature>(setNotification);

        return setNotification;
    }

    private class RequestHttpApplicationEventsFeature : IHttpApplicationEventsFeature, INotificationFeature, IHttpResponseAdapterFeature
    {
        private readonly IHttpApplicationEventsFeature _app;
        private readonly HttpContextCore _context;
        private readonly FeatureReference<IHttpResponseAdapterFeature> _response;

        public RequestHttpApplicationEventsFeature(IHttpApplicationEventsFeature app, HttpContextCore context)
        {
            _app = app;
            _context = context;
            _response = FeatureReference<IHttpResponseAdapterFeature>.Default;
        }

        public RequestNotification CurrentNotification { get; set; }

        public bool IsPostNotification { get; set; }

        /// <summary>
        /// Gets whether End() has been called. We access it via features as it may be replaced later on and we want the actual value of it
        /// </summary>
        private bool IsEnded => _response.Fetch(_context.Features) is { IsEnded: true };

        async ValueTask IHttpApplicationEventsFeature.RaiseAcquireRequestStateAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetNotification(RequestNotification.AcquireRequestState);

            await _app.RaiseAcquireRequestStateAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseAuthenticateRequestAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetNotification(RequestNotification.AuthenticateRequest);

            await _app.RaiseAuthenticateRequestAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseAuthorizeRequestAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetNotification(RequestNotification.AuthorizeRequest);

            await _app.RaiseAuthorizeRequestAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseBeginRequestAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetNotification(RequestNotification.BeginRequest);

            await _app.RaiseBeginRequestAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseEndRequestAsync(CancellationToken token)
        {
            // Unlike other events, we raise EndRequest event even when IsEnded == true
            SetNotification(RequestNotification.EndRequest);

            await _app.RaiseEndRequestAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseErrorAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            if (_app is not null)
            {
                await _app.RaiseErrorAsync(token);
            }
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseLogRequestAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetNotification(RequestNotification.LogRequest);

            await _app.RaiseLogRequestAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseMapRequestHandlerAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetNotification(RequestNotification.MapRequestHandler);

            await _app.RaiseMapRequestHandlerAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostAcquireRequestStateAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetPostNotification(RequestNotification.AcquireRequestState);

            await _app.RaisePostAcquireRequestStateAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostAuthenticateRequestAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetPostNotification(RequestNotification.AuthenticateRequest);

            await _app.RaisePostAuthenticateRequestAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostAuthorizeRequestAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetPostNotification(RequestNotification.AuthorizeRequest);

            await _app.RaisePostAuthorizeRequestAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostLogRequestAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetPostNotification(RequestNotification.LogRequest);

            await _app.RaisePostLogRequestAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostMapRequestHandlerAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetPostNotification(RequestNotification.MapRequestHandler);

            await _app.RaisePostMapRequestHandlerAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostReleaseRequestStateAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetPostNotification(RequestNotification.ReleaseRequestState);

            await _app.RaisePostReleaseRequestStateAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostRequestHandlerExecuteAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetPostNotification(RequestNotification.ExecuteRequestHandler);

            await _app.RaisePostRequestHandlerExecuteAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostResolveRequestCacheAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetPostNotification(RequestNotification.ResolveRequestCache);

            await _app.RaisePostResolveRequestCacheAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePostUpdateRequestCacheAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetPostNotification(RequestNotification.UpdateRequestCache);

            await _app.RaisePostUpdateRequestCacheAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaisePreRequestHandlerExecuteAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetNotification(RequestNotification.PreExecuteRequestHandler);

            await _app.RaisePreRequestHandlerExecuteAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseReleaseRequestStateAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetNotification(RequestNotification.ReleaseRequestState);

            await _app.RaiseReleaseRequestStateAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseRequestCompletedAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetNotification(RequestNotification.EndRequest);

            await _app.RaiseRequestCompletedAsync(token);
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseResolveRequestCacheAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetNotification(RequestNotification.ResolveRequestCache);

            await _app.RaiseResolveRequestCacheAsync(token);
        }

        public ValueTask RaiseSessionEnd(CancellationToken token)
        {
            if (IsEnded)
            {
                return ValueTask.CompletedTask;
            }

            if (_app is not null)
            {
                return _app.RaiseSessionEnd(token);
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask RaiseSessionStart(CancellationToken token)
        {
            if (IsEnded)
            {
                return ValueTask.CompletedTask;
            }

            if (_app is not null)
            {
                return _app.RaiseSessionStart(token);
            }

            return ValueTask.CompletedTask;
        }

        async ValueTask IHttpApplicationEventsFeature.RaiseUpdateRequestCacheAsync(CancellationToken token)
        {
            if (IsEnded)
            {
                return;
            }

            SetNotification(RequestNotification.UpdateRequestCache);

            await _app.RaiseUpdateRequestCacheAsync(token);
        }

        ValueTask IHttpApplicationEventsFeature.RaisePreSendRequestHeaders(CancellationToken token)
        {
            if (IsEnded)
            {
                return ValueTask.CompletedTask;
            }

            if (_app is not null)
            {
                return _app.RaisePreSendRequestHeaders(token);
            }

            return ValueTask.CompletedTask;
        }

        private void SetPostNotification(RequestNotification notification)
        {
            if (IsEnded)
            {
                return;
            }

            SetNotification(notification, true);
        }

        private void SetNotification(RequestNotification notification, bool isPost = false)
        {
            CurrentNotification = notification;
            IsPostNotification = isPost;
        }

        private bool _isEnded;

        bool IHttpResponseAdapterFeature.IsEnded => _isEnded;

        bool IHttpResponseAdapterFeature.SuppressContent
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        Task IHttpResponseAdapterFeature.EndAsync()
        {
            _isEnded = true;
            return _context.Response.CompleteAsync();
        }

        void IHttpResponseAdapterFeature.ClearContent()
            => throw new NotSupportedException();
    }
}
