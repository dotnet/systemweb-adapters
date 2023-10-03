// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class SetHttpContextTimestampMiddleware
{
    private readonly RequestDelegate _next;

#if NET8_0_OR_GREATER
    private readonly TimeProvider _time;
#endif

    public SetHttpContextTimestampMiddleware(
#if NET8_0_OR_GREATER
        TimeProvider time,
#endif
        RequestDelegate next)
    {
#if NET8_0_OR_GREATER
        _time = time;
#endif
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
#if NET8_0_OR_GREATER
        var dt = _time.GetLocalNow();
#else
        var dt = DateTime.UtcNow.ToLocalTime();
#endif
        context.Features.Set<TimestampFeature>(new(dt));

        return _next(context);
    }

    private sealed class TimestampFeature : ITimestampFeature
    {
        public TimestampFeature(DateTimeOffset dt)
            => Timestamp = dt;

        public DateTimeOffset Timestamp { get; }
    }
}
