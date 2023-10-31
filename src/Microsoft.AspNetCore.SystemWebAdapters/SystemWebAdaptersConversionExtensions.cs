// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class SystemWebAdaptersConversionExtensions
{
    /// <summary>
    /// Gets a representation of the <see cref="HttpContextCore"/> as an <see cref="HttpContext"/>. For any given
    /// instance of <see cref="HttpContextCore"/>, only a single <see cref="HttpContext"/> will be returned.
    /// </summary>
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

    /// <summary>
    /// Gets a representation of the <see cref="HttpRequestCore"/> as an <see cref="HttpRequest"/>. For any given
    /// instance of <see cref="HttpRequestCore"/>, only a single <see cref="HttpRequest"/> will be returned.
    /// </summary>
    public static HttpRequest AsSystemWeb(this HttpRequestCore request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request;
    }

    /// <summary>
    /// Gets a representation of the <see cref="HttpResponseCore"/> as an <see cref="HttpResponse"/>. For any given
    /// instance of <see cref="HttpResponseCore"/>, only a single <see cref="HttpResponse"/> will be returned.
    /// </summary>
    public static HttpResponse AsSystemWeb(this HttpResponseCore response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return response.HttpContext.AsSystemWeb().Response;
    }

    /// <summary>
    /// Gets a representation of the <see cref="HttpContext"/> as an <see cref="HttpContextCore"/>. For any given
    /// instance of <see cref="HttpContext"/>, only a single <see cref="HttpContextCore"/> will be returned.
    /// </summary>
    public static HttpContextCore AsAspNetCore(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context;
    }

    /// <summary>
    /// Gets a representation of the <see cref="HttpRequest"/> as an <see cref="HttpRequestCore"/>. For any given
    /// instance of <see cref="HttpRequest"/>, only a single <see cref="HttpRequestCore"/> will be returned.
    /// </summary>
    public static HttpRequestCore AsAspNetCore(this HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request;
    }

    /// <summary>
    /// Gets a representation of the <see cref="HttpResponse"/> as an <see cref="HttpResponseCore"/>. For any given
    /// instance of <see cref="HttpResponse"/>, only a single <see cref="HttpResponseCore"/> will be returned.
    /// </summary>
    public static HttpResponseCore AsAspNetCore(this HttpResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return response;
    }

    internal static HttpContextBase AsSystemWebBase(this HttpContextCore context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = context.Features.Get<HttpContextBase>();

        if (result is null)
        {
            result = new HttpContextWrapper(context.AsSystemWeb());
            context.Features.Set(result);
        }

        return result;
    }
}
