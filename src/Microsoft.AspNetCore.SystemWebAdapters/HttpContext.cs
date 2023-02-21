// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.SessionState;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
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

    public static HttpContext? Current => _accessor.HttpContext;

    internal HttpContext(HttpContextCore context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public HttpRequest Request => _request ??= new(_context.Request);

    public HttpResponse Response => _response ??= new(_context.Response);

    public IDictionary Items => _items ??= _context.Items.AsNonGeneric();

    public HttpServerUtility Server => _server ??= new(_context);

    public Cache Cache => _context.RequestServices.GetRequiredService<Cache>();

    /// <summary>
    /// Gets whether the current request is running in the development environment.
    /// </summary>
    public bool IsDebuggingEnabled => _context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();

    public IPrincipal User
    {
        get => _context.User;
        set => _context.User = value is ClaimsPrincipal claims ? claims : new ClaimsPrincipal(value);
    }

    public IHttpHandler? Handler
    {
        get => _context.Features.GetRequired<IHttpHandlerFeature>().Current;
        set => _context.Features.GetRequired<IHttpHandlerFeature>().Current = value;
    }

    public IHttpHandler? CurrentHandler => Handler;

    public IHttpHandler? PreviousHandler => _context.Features.GetRequired<IHttpHandlerFeature>().Previous;

    public HttpSessionState? Session => _context.Features.Get<HttpSessionState>();

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = Constants.ApiFromAspNet)]
    public void ClearError()
    {
        // Intentionally implemented without any behavior as there's nothing equivalent in ASP.NET Core
    }

    public DateTime Timestamp { get; } = DateTime.UtcNow.ToLocalTime();

    public void RewritePath(string path)
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

        _context.Request.QueryString = new QueryString(qs);
        _context.Request.Path = new PathString(path.Trim());
    }

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

    [return: NotNullIfNotNull("context")]
    public static implicit operator HttpContext?(HttpContextCore? context) => context?.GetAdapter();

    [return: NotNullIfNotNull("context")]
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
