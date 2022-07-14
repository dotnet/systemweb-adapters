// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class SingleThreadedRequestMiddleware
{
    private readonly RequestDelegate _next;

    public SingleThreadedRequestMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContextCore context)
        => context.GetEndpoint()?.Metadata.GetMetadata<SingleThreadedRequestAttribute>() is { IsEnabled: true }
            ? EnsureSingleThreaded(context)
            : _next(context);

    private Task EnsureSingleThreaded(HttpContextCore context)
    {
        var schedule = new ConcurrentExclusiveSchedulerPair(TaskScheduler.Default, 1);

        return Task.Factory.StartNew(() => _next(context), context.RequestAborted, TaskCreationOptions.DenyChildAttach, schedule.ExclusiveScheduler).Unwrap();
    }
}
