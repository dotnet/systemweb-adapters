// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class SetHttpContextTimestampMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TimeProvider _timeProvider;

    public SetHttpContextTimestampMiddleware(TimeProvider timeProvider, RequestDelegate next)
    {
        _timeProvider = timeProvider;
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        context.Features.Set<TimestampFeature>(new(_timeProvider));

        return _next(context);
    }

    private sealed class TimestampFeature : ITimestampFeature
    {
        public TimestampFeature(TimeProvider timeProvider)
            => Timestamp = timeProvider.GetLocalNow();

        public DateTimeOffset Timestamp { get; }
    }
}
