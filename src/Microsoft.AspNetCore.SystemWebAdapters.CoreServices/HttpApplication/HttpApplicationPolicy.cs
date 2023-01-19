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
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// According to https://learn.microsoft.com/en-us/dotnet/api/system.web.httpapplication?view=netframework-4.8.1#remarks,
/// a single <see cref="HttpApplication"/> instance can only be used by a single request at a time. This sets up a pool
/// to keep instances available for reuse.
/// </summary>
internal partial class HttpApplicationPolicy : PooledObjectPolicy<HttpApplication>
{
    [LoggerMessage(0, LogLevel.Debug, "Created HttpApplication instance ({Count})")]
    partial void HttpApplicationCreated(long count);

    private readonly IServiceProvider _sp;
    private readonly ILogger _logger;
    private readonly IOptions<HttpApplicationOptions> _options;

    private long _count;

    public HttpApplicationPolicy(
        IServiceProvider sp,
        IOptions<HttpApplicationOptions> options,
        ILogger<HttpApplicationPolicy> logger)
    {
        _sp = sp;
        _logger = logger;
        _options = options;
        _count = 0;
    }

    public override HttpApplication Create()
    {
        var app = _options.Value.Factory(_sp);

        var count = Interlocked.Increment(ref _count);
        HttpApplicationCreated(count);

        return app;
    }

    public override bool Return(HttpApplication obj)
    {
        obj.Context = null!;
        return true;
    }
}
