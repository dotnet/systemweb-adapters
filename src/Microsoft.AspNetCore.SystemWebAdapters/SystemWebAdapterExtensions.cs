// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class SystemWebAdapterExtensions
{
    [return: NotNullIfNotNull(nameof(request))]
    public static HttpRequest? GetAdapter(this HttpRequestCore? request)
        => request?.HttpContext.GetAdapter().Request;

    [return: NotNullIfNotNull(nameof(request))]
    public static HttpRequestBase? GetAdapterBase(this HttpRequestCore? request)
        => request?.HttpContext.GetAdapterBase().Request;

    [return: NotNullIfNotNull(nameof(request))]
    public static HttpRequestCore? UnwrapAdapter(this HttpRequest? request) => request;

    [return: NotNullIfNotNull(nameof(response))]
    public static HttpResponse? GetAdapter(this HttpResponseCore? response)
        => response?.HttpContext.GetAdapter().Response;

    [return: NotNullIfNotNull(nameof(response))]
    public static HttpResponseBase? GetAdapterBase(this HttpResponseCore? response)
        => response?.HttpContext.GetAdapterBase().Response;

    [return: NotNullIfNotNull(nameof(response))]
    public static HttpResponseCore? UnwrapAdapter(this HttpResponse? response) => response;

    [return: NotNullIfNotNull(nameof(context))]
    public static HttpContext? GetAdapter(this HttpContextCore? context)
    {
        if (context is null)
        {
            return null;
        }

        var result = context.Features.Get<HttpContext>();

        if (result is null)
        {
            result = new(context);
            context.Features.Set(result);
        }

        return result;
    }

    [return: NotNullIfNotNull(nameof(context))]
    public static HttpContextCore? UnwrapAdapter(this HttpContext? context) => context;

    [return: NotNullIfNotNull(nameof(context))]
    public static HttpContextBase? GetAdapterBase(this HttpContextCore? context)
    {
        if (context is null)
        {
            return null;
        }

        var result = context.Features.Get<HttpContextBase>();

        if (result is null)
        {
            result = new HttpContextWrapper(context);
            context.Features.Set(result);
        }

        return result!;
    }

    internal static TFeature GetRequired<TFeature>(this IFeatureCollection features)
    {
        if (features.Get<TFeature>() is TFeature feature)
        {
            return feature;
        }

        throw new InvalidOperationException($"Feature {typeof(TFeature)} is not available");
    }
}
