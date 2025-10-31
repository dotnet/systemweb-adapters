// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Owin;

namespace System.Web;

public static class HttpContextExtensions
{
    /// <summary>
    /// Gets an <see cref="IOwinContext"/> from the given <see cref="HttpContext"/>."/>
    /// </summary>
    public static IOwinContext GetOwinContext(this HttpContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        return ctx.AsAspNetCore().GetOwinContext();
    }

    /// <summary>
    /// Gets an <see cref="IOwinContext"/> from the given <see cref="HttpRequest"/>."/>
    /// </summary>
    public static IOwinContext GetOwinContext(this HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.AsAspNetCore().HttpContext.GetOwinContext();
    }
}
