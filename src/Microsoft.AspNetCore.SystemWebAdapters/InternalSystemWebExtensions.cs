// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Internal;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static class InternalSystemWebExtensions
{
    [return: NotNullIfNotNull(nameof(request))]
    internal static HttpRequest? GetAdapter(this HttpRequestCore? request)
        => request?.HttpContext.GetAdapter().Request;

    [return: NotNullIfNotNull(nameof(request))]
    internal static HttpRequestBase? GetAdapterBase(this HttpRequestCore? request)
        => request?.HttpContext.GetAdapterBase().Request;

    [return: NotNullIfNotNull(nameof(request))]
    internal static HttpRequestCore? UnwrapAdapter(this HttpRequest? request) => request;

    [return: NotNullIfNotNull(nameof(response))]
    internal static HttpResponse? GetAdapter(this HttpResponseCore? response)
        => response?.HttpContext.GetAdapter().Response;

    [return: NotNullIfNotNull("request")]
    internal static HttpResponseBase? GetAdapterBase(this HttpResponseCore? response)
        => response?.HttpContext.GetAdapterBase().Response;

    [return: NotNullIfNotNull(nameof(response))]
    internal static HttpResponseCore? UnwrapAdapter(this HttpResponse? response) => response;

    internal static IDictionary AsNonGeneric(this IDictionary<object, object?> dictionary)
         => dictionary is IDictionary d ? d : new NonGenericDictionaryWrapper(dictionary);

    internal static ICollection AsNonGeneric<T>(this ICollection<T> collection)
        => collection is ICollection c ? c : new NonGenericCollectionWrapper<T>(collection);

    [return: NotNullIfNotNull(nameof(context))]
    internal static HttpContext? GetAdapter(this HttpContextCore? context)
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
    internal static HttpContextCore? UnwrapAdapter(this HttpContext? context) => context;

    [return: NotNullIfNotNull(nameof(context))]
    internal static HttpContextBase? GetAdapterBase(this HttpContextCore? context)
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
