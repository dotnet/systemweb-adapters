// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// System.Web would set some response headers by default. This adds those early in the pipeline so they can still be overridden. If they exist, they will not be replaced.
/// </summary>
internal class SetDefaultResponseHeadersMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly (string Header, string Value)[] _defaultHeaders = new (string, string)[]
    {
        (HeaderNames.CacheControl, CacheControlHeaderValue.PrivateString),
        (HeaderNames.ContentType, "text/html")
    };

    public SetDefaultResponseHeadersMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(static state =>
        {
            var context = (HttpContext)state;

            foreach (var (header, value) in _defaultHeaders)
            {
                if (!context.Response.Headers.ContainsKey(header))
                {
                    context.Response.Headers[header] = value;
                }
            }

            return Task.CompletedTask;
        }, context);

        return _next(context);
    }
}
