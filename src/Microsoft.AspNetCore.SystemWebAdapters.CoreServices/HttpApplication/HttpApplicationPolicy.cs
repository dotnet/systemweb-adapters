// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// According to https://learn.microsoft.com/en-us/dotnet/api/system.web.httpapplication?view=netframework-4.8.1#remarks,
/// a single <see cref="HttpApplication"/> instance can only be used by a single request at a time. This sets up a pool
/// to keep instances available for reuse.
/// </summary>
internal partial class HttpApplicationPolicy<TApp> : PooledObjectPolicy<HttpApplication>
    where TApp : HttpApplication
{
    [LoggerMessage(0, LogLevel.Debug, "Created HttpApplication instance ({Count})")]
    partial void HttpApplicationCreated(long count);

    private readonly HttpApplicationState _state;
    private readonly IServiceProvider _sp;
    private readonly ILogger _logger;
    private readonly HttpApplicationEventFactory _eventInitializer;
    private readonly ObjectFactory _factory;
    private long _count;

    public HttpApplicationPolicy(
        HttpApplicationState state,
        IServiceProvider sp,
        HttpApplicationEventFactory eventInitializer,
        ILogger<HttpApplicationPolicy<TApp>> logger)
    {
        _state = state;
        _sp = sp;
        _logger = logger;
        _eventInitializer = eventInitializer;
        _count = 0;
        _factory = ActivatorUtilities.CreateFactory(typeof(TApp), Array.Empty<Type>());
    }

    public override HttpApplication Create()
    {
        var app = (HttpApplication)_factory(_sp, null);
        var modules = _sp.GetRequiredService<IEnumerable<IHttpModule>>().ToList();
        var count = Interlocked.Increment(ref _count);

        app.Application = _state;
        _eventInitializer.InitializeEvents(app);
        app.Initialize(modules);

        HttpApplicationCreated(count);

        return app;
    }

    public override bool Return(HttpApplication obj)
    {
        obj.Context = null!;
        return true;
    }
}
