// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class SetHttpContextTimestampMiddleware
{
    private readonly RequestDelegate _next;

    public SetHttpContextTimestampMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        // Ensure adapter is created to force timestamp to be set
        _ = context.GetSystemWebHttpContext();

        return _next(context);
    }
}
