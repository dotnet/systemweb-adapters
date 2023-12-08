// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// System.Web would set some response headers by default. This adds those early in the pipeline so they can still be overridden. If they exist, they will not be replaced.
/// </summary>
internal class SetDefaultResponseHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SetDefaultResponseHeadersMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(static state =>
        {
            var context = (HttpContext)state;

            WriteDefaultContentType(context);
            context.Response.AsSystemWeb().Cache.UpdateHeaders();

            return Task.CompletedTask;
        }, context);

        return _next(context);
    }

    private static void WriteDefaultContentType(HttpContext context)
    {
        if (context.Response.ContentLength.HasValue && context.Response.Headers.ContentType.Count == 0)
        {
            context.Response.Headers.ContentType = "text/html";
        }
    }
}
