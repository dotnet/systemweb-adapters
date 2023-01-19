// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web;

[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = Constants.ApiFromAspNet)]
public class HttpApplication : IDisposable, IHttpApplicationEventsFeature
{
    private IHttpModule[]? _modules;
    private HttpApplicationState _state = null!;
    private HttpContext? _context;

    public HttpApplication()
    {
    }

    internal void Initialize(IHttpModule[] modules, HttpApplicationState state, Action<HttpApplication> eventInitializer)
    {
        _modules = modules;
        _state = state;

        eventInitializer(this);

        if (_modules is null)
        {
            return;
        }

        foreach (var m in _modules)
        {
            m.Init(this);
        }
    }

    public HttpApplicationState Application
        => _state ?? throw new InvalidOperationException("Can only be accessed during valid requests");

    public HttpContext Context
    {
        get => _context ?? throw new InvalidOperationException("Can only be accessed during valid request.");
        internal set => _context = value;
    }

    public HttpRequest Request => Context.Request;

    public HttpResponse Response => Context.Response;

    public HttpServerUtility Server => Context.Server;

    [Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
    public HttpSessionState Session => Context.Session ?? throw new HttpException("Session is not available");

    public IPrincipal User => Context.User;

    public void CompleteRequest() => Context.Response.End();

    public event EventHandler? BeginRequest;

    ValueTask IHttpApplicationEventsFeature.RaiseBeginRequestAsync(CancellationToken token)
    {
        BeginRequest?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? AuthenticateRequest;

    ValueTask IHttpApplicationEventsFeature.RaiseAuthenticateRequestAsync(CancellationToken token)
    {
        AuthenticateRequest?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostAuthenticateRequest;

    ValueTask IHttpApplicationEventsFeature.RaisePostAuthenticateRequestAsync(CancellationToken token)
    {
        PostAuthenticateRequest?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? AuthorizeRequest;

    ValueTask IHttpApplicationEventsFeature.RaiseAuthorizeRequestAsync(CancellationToken token)
    {
        AuthorizeRequest?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostAuthorizeRequest;

    ValueTask IHttpApplicationEventsFeature.RaisePostAuthorizeRequestAsync(CancellationToken token)
    {
        PostAuthorizeRequest?.Invoke(this, EventArgs.Empty);

        return ValueTask.CompletedTask;
    }

    public event EventHandler? ResolveRequestCache;

    ValueTask IHttpApplicationEventsFeature.RaiseResolveRequestCacheAsync(CancellationToken token)
    {
        ResolveRequestCache?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostResolveRequestCache;

    ValueTask IHttpApplicationEventsFeature.RaisePostResolveRequestCacheAsync(CancellationToken token)
    {
        PostResolveRequestCache?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? MapRequestHandler;

    ValueTask IHttpApplicationEventsFeature.RaiseMapRequestHandlerAsync(CancellationToken token)
    {
        MapRequestHandler?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostMapRequestHandler;

    ValueTask IHttpApplicationEventsFeature.RaisePostMapRequestHandlerAsync(CancellationToken token)
    {
        PostMapRequestHandler?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? AcquireRequestState;

    ValueTask IHttpApplicationEventsFeature.RaiseAcquireRequestStateAsync(CancellationToken token)
    {
        AcquireRequestState?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostAcquireRequestState;

    ValueTask IHttpApplicationEventsFeature.RaisePostAcquireRequestStateAsync(CancellationToken token)
    {
        PostAcquireRequestState?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PreRequestHandlerExecute;

    ValueTask IHttpApplicationEventsFeature.RaisePreRequestHandlerExecuteAsync(CancellationToken token)
    {
        PreRequestHandlerExecute?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostRequestHandlerExecute;

    ValueTask IHttpApplicationEventsFeature.RaisePostRequestHandlerExecuteAsync(CancellationToken token)
    {
        PostRequestHandlerExecute?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? ReleaseRequestState;

    ValueTask IHttpApplicationEventsFeature.RaiseReleaseRequestStateAsync(CancellationToken token)
    {
        ReleaseRequestState?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostReleaseRequestState;

    ValueTask IHttpApplicationEventsFeature.RaisePostReleaseRequestStateAsync(CancellationToken token)
    {
        PostReleaseRequestState?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? UpdateRequestCache;

    ValueTask IHttpApplicationEventsFeature.RaiseUpdateRequestCacheAsync(CancellationToken token)
    {
        UpdateRequestCache?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostUpdateRequestCache;

    ValueTask IHttpApplicationEventsFeature.RaisePostUpdateRequestCacheAsync(CancellationToken token)
    {
        PostUpdateRequestCache?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? LogRequest;

    ValueTask IHttpApplicationEventsFeature.RaiseLogRequestAsync(CancellationToken token)
    {
        LogRequest?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostLogRequest;

    ValueTask IHttpApplicationEventsFeature.RaisePostLogRequestAsync(CancellationToken token)
    {
        PostLogRequest?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? EndRequest;

    ValueTask IHttpApplicationEventsFeature.RaiseEndRequestAsync(CancellationToken token)
    {
        EndRequest?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? Error;

    ValueTask IHttpApplicationEventsFeature.RaiseErrorAsync(CancellationToken token)
    {
        Error?.Invoke(this, EventArgs.Empty);

        return ValueTask.CompletedTask;
    }

    public event EventHandler? RequestCompleted;

    ValueTask IHttpApplicationEventsFeature.RaiseRequestCompletedAsync(CancellationToken token)
    {
        RequestCompleted?.Invoke(this, EventArgs.Empty);

        return ValueTask.CompletedTask;
    }

    public event EventHandler? Disposed;

    [Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = Constants.ApiFromAspNet)]
    public void Dispose()
    {
        Disposed?.Invoke(this, EventArgs.Empty);

        if (_modules is { } modules)
        {
            foreach (var module in modules)
            {
                module.Dispose();
            }
        }
    }

    internal EventHandler? SessionStart { get; set; }

    ValueTask IHttpApplicationEventsFeature.RaiseSessionStart(CancellationToken token)
    {
        SessionStart?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    internal EventHandler? SessionEnd { get; set; }

    ValueTask IHttpApplicationEventsFeature.RaiseSessionEnd(CancellationToken token)
    {
        SessionEnd?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PreSendRequestHeaders;

    ValueTask IHttpApplicationEventsFeature.RaisePreSendRequestHeaders(CancellationToken token)
    {
        PreSendRequestHeaders?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }
}
