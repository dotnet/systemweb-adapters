// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// System.Web would automatically set the Cache-Control header to private.
/// </summary>
internal class DefaultCacheControlMiddleware
{
    private readonly RequestDelegate _next;

    public DefaultCacheControlMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (!context.Response.Headers.TryGetValue(HeaderNames.CacheControl, out _))
        {
            context.Response.Headers[HeaderNames.CacheControl] = CacheControlHeaderValue.PrivateString;
        }

        return _next(context);
    }
}
