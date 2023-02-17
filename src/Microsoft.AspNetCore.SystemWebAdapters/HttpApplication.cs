// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web;

[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = Constants.ApiFromAspNet)]
public class HttpApplication : IDisposable
{
    private IHttpModule[]? _modules;
    private HttpApplicationState _state = null!;
    private HttpContext? _context;

    public HttpApplication()
    {
        Events = new(this);
    }

    internal EventCollection Events { get; }

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

    public event EventHandler? BeginRequest
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? AuthenticateRequest
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? PostAuthenticateRequest
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? AuthorizeRequest
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? PostAuthorizeRequest
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? ResolveRequestCache
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? PostResolveRequestCache
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? MapRequestHandler
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? PostMapRequestHandler
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? AcquireRequestState
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? PostAcquireRequestState
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? PreRequestHandlerExecute
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? PostRequestHandlerExecute
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? ReleaseRequestState
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? PostReleaseRequestState
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? UpdateRequestCache
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? PostUpdateRequestCache
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? LogRequest
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? PostLogRequest
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? EndRequest
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? Error
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? RequestCompleted
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? Disposed
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = Constants.ApiFromAspNet)]
    public void Dispose()
    {
        Events.Invoke(ApplicationEvent.Disposed);

        if (_modules is { } modules)
        {
            foreach (var module in modules)
            {
                module.Dispose();
            }
        }
    }

    internal event EventHandler? SessionStart
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    internal event EventHandler? SessionEnd
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    public event EventHandler? PreSendRequestHeaders
    {
        add => Events.Add(value);
        remove => Events.Remove(value);
    }

    internal sealed class EventCollection
    {
        private readonly Dictionary<ApplicationEvent, EventHandler> _events = new();
        private readonly HttpApplication _app;

        public EventCollection(HttpApplication app)
        {
            _app = app;
        }

        public void Add(EventHandler? handler, [CallerMemberName] string? name = null)
        {
            if (handler is null)
            {
                return;
            }

            if (Enum.TryParse<ApplicationEvent>(name, out var eventName))
            {
                Add(eventName, handler);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }
        }

        public void Remove(EventHandler? handler, [CallerMemberName] string? name = null)
        {
            if (handler is null)
            {
                return;
            }

            if (Enum.TryParse<ApplicationEvent>(name, out var eventName))
            {
                Remove(eventName, handler);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }
        }


        public void Add(ApplicationEvent appEvent, EventHandler handler)
        {
            if (_events.TryGetValue(appEvent, out var existing))
            {
                _events[appEvent] = existing + handler;
            }
            else
            {
                _events.Add(appEvent, handler);
            }
        }

        public void Remove(ApplicationEvent appEvent, EventHandler handler)
        {
            if (_events.TryGetValue(appEvent, out var existing))
            {
                if (existing - handler is { } updated)
                {
                    _events[appEvent] = updated;
                }
                else
                {
                    _events.Remove(appEvent);
                }
            }
        }

        public void Invoke(ApplicationEvent appEvent)
        {
            if (_events.TryGetValue(appEvent, out var @event))
            {
                @event(_app, EventArgs.Empty);
            }
        }
    }
}
