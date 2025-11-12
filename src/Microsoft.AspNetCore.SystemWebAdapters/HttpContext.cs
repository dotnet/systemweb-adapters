// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.SessionState;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.Web;

public class HttpContext : IServiceProvider
{
    private HttpRequest? _request;
    private HttpResponse? _response;
    private HttpServerUtility? _server;
    private IDictionary? _items;
    private TraceContext? _trace;

    public static HttpContext? Current
    {
        get => HostingEnvironmentAccessor.HttpContextAccessor.HttpContext?.AsSystemWeb();
        set => HostingEnvironmentAccessor.HttpContextAccessor.HttpContext = value?.AsAspNetCore();
    }

    internal HttpContext(HttpContextCore context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    internal HttpContextCore Context { get; }

    public HttpRequest Request => _request ??= new(Context.Request);

    public HttpResponse Response => _response ??= new(Context.Response);

    public IDictionary Items
    {
        get
        {
            if (_items is null)
            {
                var items = Context.Items;
                _items = items is IDictionary d ? d : new NonGenericDictionaryWrapper(items);
            }

            return _items;
        }
    }

    public HttpServerUtility Server => _server ??= new(Context);

    public TraceContext Trace => _trace ??= new(Context);

    public Exception? Error => Context.Features.Get<IRequestExceptionFeature>()?.Exceptions is [{ } error, ..] ? error : null;

    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = Constants.ApiFromAspNet)]
    public Exception[] AllErrors => Context.Features.Get<IRequestExceptionFeature>()?.Exceptions.ToArray() ?? Array.Empty<Exception>();

    public void ClearError() => Context.Features.Get<IRequestExceptionFeature>()?.Clear();

    public void AddError(Exception ex) => Context.Features.Get<IRequestExceptionFeature>()?.Add(ex);

    public RequestNotification CurrentNotification => Context.Features.GetRequiredFeature<IHttpApplicationFeature>().CurrentNotification;

    public bool IsPostNotification => Context.Features.GetRequiredFeature<IHttpApplicationFeature>().IsPostNotification;

    public HttpApplication ApplicationInstance => Context.Features.GetRequiredFeature<IHttpApplicationFeature>().Application;

    public HttpApplicationState Application => ApplicationInstance.Application;

    public Cache Cache => Context.RequestServices.GetRequiredService<Cache>();

    public IHttpHandler? Handler
    {
        get => Context.Features.GetRequiredFeature<IHttpHandlerFeature>().Current;
        set => Context.Features.GetRequiredFeature<IHttpHandlerFeature>().Current = value;
    }

    public IHttpHandler? CurrentHandler => Handler;

    public IHttpHandler? PreviousHandler => Context.Features.GetRequiredFeature<IHttpHandlerFeature>().Previous;

    public void RemapHandler(IHttpHandler handler) => Handler = handler;

    /// <summary>
    /// Gets whether the current request is running in the development environment.
    /// </summary>
    public bool IsDebuggingEnabled => Context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();

    public IPrincipal User
    {
        get => Context.Features.Get<IRequestUserFeature>()?.User ?? Context.User;
        set => Context.Features.GetRequiredFeature<IRequestUserFeature>().User = value;
    }

    public HttpSessionState? Session => Context.Features.Get<ISessionStateFeature>()?.Session;

    public void SetSessionStateBehavior(SessionStateBehavior sessionStateBehavior)
        => Context.Features.GetRequiredFeature<ISessionStateFeature>().Behavior = sessionStateBehavior;

    public DateTime Timestamp => Context.Features.GetRequiredFeature<ITimestampFeature>().Timestamp.DateTime;

    public void RewritePath(string path) => RewritePath(path, true);

    public void RewritePath(string path, bool rebaseClientPath)
    {
        ArgumentNullException.ThrowIfNull(path);

        // Extract query string
        string? qs = null;
        var iqs = path.IndexOf('?', StringComparison.Ordinal);

        if (iqs >= 0)
        {
            qs = (iqs < path.Length - 1) ? path[iqs..] : string.Empty;
            path = path[..iqs];
        }

        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        RewritePath(path.Trim(), string.Empty, qs, rebaseClientPath);
    }

    public void RewritePath(string filePath, string pathInfo, string? queryString)
        => RewritePath(filePath, pathInfo, queryString, false);

    public void RewritePath(string filePath, string pathInfo, string? queryString, bool setClientFilePath)
        => Context.Features.GetRequiredFeature<IHttpRequestPathFeature>().Rewrite(filePath, pathInfo, queryString, setClientFilePath);

    [SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = Constants.ApiFromAspNet)]
    object? IServiceProvider.GetService(Type service)
    {
        if (service == typeof(HttpRequest))
        {
            return Request;
        }
        else if (service == typeof(HttpResponse))
        {
            return Response;
        }
        else if (service == typeof(HttpSessionState))
        {
            return Session;
        }
        else if (service == typeof(HttpServerUtility))
        {
            return Server;
        }

        return Context.RequestServices?.GetService(service);
    }

    public ISubscriptionToken DisposeOnPipelineCompleted(IDisposable target)
    {
        var token = new DisposeOnPipelineSubscriptionToken(target);
        Context.Response.RegisterForDispose(token);
        return token;
    }

    [return: NotNullIfNotNull(nameof(context))]
    public static implicit operator HttpContext?(HttpContextCore? context) => context?.AsSystemWeb();

    [return: NotNullIfNotNull(nameof(context))]
    public static implicit operator HttpContextCore?(HttpContext? context) => context?.AsAspNetCore();

    private sealed class DisposeOnPipelineSubscriptionToken : ISubscriptionToken, IDisposable
    {
        private IDisposable? _other;

        public DisposeOnPipelineSubscriptionToken(IDisposable other) => _other = other;

        bool ISubscriptionToken.IsActive => _other is not null;

        void ISubscriptionToken.Unsubscribe() => _other = null;

        void IDisposable.Dispose()
        {
            _other?.Dispose();
            _other = null;
        }
    }
}
