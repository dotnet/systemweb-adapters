// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class SystemWebAdapterExtensions
{
    [return: NotNullIfNotNull(nameof(request))]
    public static HttpRequest? GetSystemWebRequest(this HttpRequestCore? request)
        => request?.HttpContext.GetSystemWebHttpContext().Request;

    [return: NotNullIfNotNull(nameof(request))]
    public static HttpRequestBase? GetSystemWebRequestBase(this HttpRequestCore? request)
        => request?.HttpContext.GetSystemWebHttpContextBase().Request;

    [return: NotNullIfNotNull(nameof(request))]
    public static HttpRequestCore? GetAspNetCoreRequest(this HttpRequest? request) => request;

    [return: NotNullIfNotNull(nameof(response))]
    public static HttpResponse? GetSystemWebResponse(this HttpResponseCore? response)
        => response?.HttpContext.GetSystemWebHttpContext().Response;

    [return: NotNullIfNotNull(nameof(response))]
    public static HttpResponseBase? GetSystemWebResponseBase(this HttpResponseCore? response)
        => response?.HttpContext.GetSystemWebHttpContextBase().Response;

    [return: NotNullIfNotNull(nameof(response))]
    public static HttpResponseCore? GetAspNetCoreResponse(this HttpResponse? response) => response;

    [return: NotNullIfNotNull(nameof(context))]
    public static HttpContext? GetSystemWebHttpContext(this HttpContextCore? context)
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
    public static HttpContextCore? GetAspNetCoreHttpContext(this HttpContext? context) => context;

    [return: NotNullIfNotNull(nameof(context))]
    public static HttpContextBase? GetSystemWebHttpContextBase(this HttpContextCore? context)
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

        return result;
    }
}
