// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.SessionState;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Internal;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.Web;

public class HttpContext : IServiceProvider
{
    private static readonly HttpContextAccessor _accessor = new();

    private readonly HttpContextCore _context;

    private HttpRequest? _request;
    private HttpResponse? _response;
    private HttpServerUtility? _server;
    private IDictionary? _items;
    private TraceContext? _trace;

    public static HttpContext? Current => _accessor.HttpContext;

    internal HttpContext(HttpContextCore context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public HttpRequest Request => _request ??= new(_context.Request);

    public HttpResponse Response => _response ??= new(_context.Response);

    public IDictionary Items => _items ??= _context.Items.AsNonGeneric();

    public HttpServerUtility Server => _server ??= new(_context);

    public TraceContext Trace => _trace ??= new(_context);

    public Exception? Error => _context.Features.Get<IRequestExceptionFeature>()?.Exceptions is [{ } error, ..] ? error : null;

    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = Constants.ApiFromAspNet)]
    public Exception[] AllErrors => _context.Features.Get<IRequestExceptionFeature>()?.Exceptions.ToArray() ?? Array.Empty<Exception>();

    public void ClearError() => _context.Features.Get<IRequestExceptionFeature>()?.Clear();

    public void AddError(Exception ex) => _context.Features.Get<IRequestExceptionFeature>()?.Add(ex);

    public RequestNotification CurrentNotification => _context.Features.GetRequired<IHttpApplicationFeature>().CurrentNotification;

    public bool IsPostNotification => _context.Features.GetRequired<IHttpApplicationFeature>().IsPostNotification;

    public HttpApplication ApplicationInstance => _context.Features.GetRequired<IHttpApplicationFeature>().Application;

    public HttpApplicationState Application => ApplicationInstance.Application;

    public Cache Cache => _context.RequestServices.GetRequiredService<Cache>();

    /// <summary>
    /// Gets whether the current request is running in the development environment.
    /// </summary>
    public bool IsDebuggingEnabled => _context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();

    public IPrincipal User
    {
        get => _context.Features.Get<IPrincipalUserFeature>()?.User ?? _context.User;
        set
        {
            if (_context.Features.Get<IPrincipalUserFeature>() is { } feature)
            {
                feature.User = value;
            }
            else
            {
                var newFeature = new PrincipalUserFeature(_context) { User = value };

                _context.Features.Set<IPrincipalUserFeature>(newFeature);
                _context.Features.Set<IHttpAuthenticationFeature>(newFeature);
            }
        }
    }

    public HttpSessionState? Session => _context.Features.Get<ISessionStateFeature>()?.Session;

    public void SetSessionStateBehavior(SessionStateBehavior sessionStateBehavior)
        => _context.Features.GetRequired<ISessionStateFeature>().Behavior = sessionStateBehavior;

    public DateTime Timestamp { get; } = DateTime.UtcNow.ToLocalTime();

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

        if (!path.StartsWith("/", StringComparison.Ordinal))
        {
            path = "/" + path;
        }

        RewritePath(path.Trim(), string.Empty, qs, rebaseClientPath);
    }

    public void RewritePath(string filePath, string pathInfo, string? queryString)
        => RewritePath(filePath, pathInfo, queryString, false);

    public void RewritePath(string filePath, string pathInfo, string? queryString, bool setClientFilePath)
        => _context.Features.GetRequired<IHttpRequestPathFeature>().Rewrite(filePath, pathInfo, queryString, setClientFilePath);

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

        return null;
    }

    public ISubscriptionToken DisposeOnPipelineCompleted(IDisposable target)
    {
        var token = new DisposeOnPipelineSubscriptionToken(target);
        _context.Response.RegisterForDispose(token);
        return token;
    }

    [return: NotNullIfNotNull(nameof(context))]
    public static implicit operator HttpContext?(HttpContextCore? context) => context?.GetAdapter();

    [return: NotNullIfNotNull(nameof(context))]
    public static implicit operator HttpContextCore?(HttpContext? context) => context?._context;

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
