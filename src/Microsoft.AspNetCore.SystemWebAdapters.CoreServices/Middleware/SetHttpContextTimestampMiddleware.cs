// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters.Middleware;

internal class SetHttpContextTimestampMiddleware(TimeProvider timeProvider, RequestDelegate next)
{
    public Task InvokeAsync(HttpContextCore context)
    {
        context.Features.Set<ITimestampFeature>(new TimestampFeature(timeProvider.GetLocalNow()));

        return next(context);
    }

    private sealed class TimestampFeature(DateTimeOffset timestamp) : ITimestampFeature
    {
        public DateTimeOffset Timestamp { get; } = timestamp;
    }
}
