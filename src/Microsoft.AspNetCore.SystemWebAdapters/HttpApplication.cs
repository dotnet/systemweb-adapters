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
    private const string HttpApplicationMustBeInitialized = "HttpApplication must be initialized before use.";

    private HttpApplicationState _state = null!;
    private HttpContext? _context;

    private Dictionary<ApplicationEvent, EventHandler>? _events;
    private HttpModuleCollection? _modules;

    public HttpApplication()
    {
    }

    internal void Initialize(HttpModuleCollection modules, HttpApplicationState state, Action<HttpApplication> eventInitializer)
    {
        _modules = modules;
        _state = state;

        eventInitializer(this);

        foreach (var module in modules.Modules)
        {
            module.Init(this);
        }
    }

    public HttpModuleCollection Modules
        => _modules ?? throw new InvalidOperationException(HttpApplicationMustBeInitialized);

    public HttpApplicationState Application
        => _state ?? throw new InvalidOperationException(HttpApplicationMustBeInitialized);

    public HttpContext Context
    {
        get => _context ?? throw new InvalidOperationException("HttpContext can only be accessed during valid request.");
        internal set => _context = value;
    }

    public HttpRequest Request => Context.Request;

    public HttpResponse Response => Context.Response;

    public HttpServerUtility Server => Context.Server;

    [Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
    public HttpSessionState Session => Context.Session ?? throw new HttpException("Session is not available");

    public IPrincipal User => Context.User;

    public void CompleteRequest() => Context.Response.End();

    public virtual string? GetVaryByCustomString(HttpContext context, string custom)
    {
        if (string.Equals(custom, "browser", StringComparison.OrdinalIgnoreCase))
        {
            return context?.Request.Browser.Type;
        }

        return null;
    }

    public event EventHandler? BeginRequest
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? AuthenticateRequest
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? PostAuthenticateRequest
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? AuthorizeRequest
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? PostAuthorizeRequest
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? ResolveRequestCache
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? PostResolveRequestCache
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? MapRequestHandler
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? PostMapRequestHandler
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? AcquireRequestState
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? PostAcquireRequestState
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? PreRequestHandlerExecute
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? PostRequestHandlerExecute
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? ReleaseRequestState
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? PostReleaseRequestState
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? UpdateRequestCache
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? PostUpdateRequestCache
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? LogRequest
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? PostLogRequest
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? EndRequest
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? Error
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? Disposed
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = Constants.ApiFromAspNet)]
    public void Dispose()
    {
        InvokeEvent(ApplicationEvent.Disposed);

        foreach (var module in Modules.Modules)
        {
            module.Dispose();
        }
    }

    internal event EventHandler? SessionStart
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    internal event EventHandler? SessionEnd
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    internal event EventHandler? ApplicationStart
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    internal event EventHandler? ApplicationInit
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    public event EventHandler? PreSendRequestHeaders
    {
        add => AddEvent(value);
        remove => RemoveEvent(value);
    }

    private void AddEvent(EventHandler? handler, [CallerMemberName] string? name = null)
    {
        if (handler is null)
        {
            return;
        }

        if (Enum.TryParse<ApplicationEvent>(name, out var eventName))
        {
            AddEvent(eventName, handler);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(name));
        }
    }

    private void RemoveEvent(EventHandler? handler, [CallerMemberName] string? name = null)
    {
        if (handler is null)
        {
            return;
        }

        if (Enum.TryParse<ApplicationEvent>(name, out var eventName))
        {
            RemoveEvent(eventName, handler);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(name));
        }
    }

    private void AddEvent(ApplicationEvent appEvent, EventHandler handler)
    {
        _events ??= new();

        if (_events.TryGetValue(appEvent, out var existing))
        {
            _events[appEvent] = existing + handler;
        }
        else
        {
            _events.Add(appEvent, handler);
        }
    }

    private void RemoveEvent(ApplicationEvent appEvent, EventHandler handler)
    {
        if (_events is null)
        {
            return;
        }

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

    internal void InvokeEvent(ApplicationEvent appEvent)
    {
        if (_events is null)
        {
            return;
        }

        if (_events.TryGetValue(appEvent, out var @event))
        {
            @event(this, EventArgs.Empty);
        }
    }
}
