// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class SystemWebAdaptersConversionExtensions
{
    public static HttpRequest AsSystemWeb(this HttpRequestCore request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.HttpContext.AsSystemWeb().Request;
    }

    public static HttpResponse AsSystemWeb(this HttpResponseCore response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return response.HttpContext.AsSystemWeb().Response;
    }

    public static HttpResponseCore AsAspNetCore(this HttpResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return response;
    }

    public static HttpContext AsSystemWeb(this HttpContextCore context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = context.Features.Get<HttpContext>();

        if (result is null)
        {
            result = new(context);
            context.Features.Set(result);
        }

        return result;
    }

    public static HttpContextCore AsAspNetCore(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context;
    }

    public static HttpRequestCore GetAspNetCoreRequest(this HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request;
    }

    internal static HttpContextBase AsSystemWebBase(this HttpContextCore context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = context.Features.Get<HttpContextBase>();

        if (result is null)
        {
            result = new HttpContextWrapper(context);
            context.Features.Set(result);
        }

        return result;
    }
}
