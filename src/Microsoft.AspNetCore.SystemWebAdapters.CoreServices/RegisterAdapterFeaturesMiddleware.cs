// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace Microsoft.Extensions.DependencyInjection;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = Constants.DisposeIsRegistered)]
internal sealed class RegisterAdapterFeaturesMiddleware
{
    private readonly RequestDelegate _next;

    public RegisterAdapterFeaturesMiddleware(RequestDelegate next)
        => _next = next;

    public async Task InvokeAsync(HttpContextCore context)
    {
        RegisterRequestFeatures(context);
        var buffering = RegisterResponseFeatures(context);

        try
        {
            await _next(context);
        }
        finally
        {
            await buffering.FlushAsync();
        }
    }

    private static void RegisterRequestFeatures(HttpContextCore context)
    {
        var existing = context.Features.GetRequired<IHttpRequestFeature>();

        var adapterFeature = new HttpRequestInputStreamFeature(existing);

        context.Response.RegisterForDispose(adapterFeature);
        context.Features.Set<IHttpRequestFeature>(adapterFeature);
        context.Features.Set<IHttpRequestInputStreamFeature>(adapterFeature);
        context.Features.Set<IRequestBodyPipeFeature>(adapterFeature);
    }

    private static IHttpResponseBufferingFeature RegisterResponseFeatures(HttpContextCore context)
    {
        var responseBodyFeature = context.Features.GetRequired<IHttpResponseBodyFeature>();

        var adapterFeature = new HttpResponseAdapterFeature(responseBodyFeature);

        context.Features.Set<IHttpResponseBodyFeature>(adapterFeature);
        context.Features.Set<IHttpResponseBufferingFeature>(adapterFeature);
        context.Features.Set<IHttpResponseEndFeature>(adapterFeature);
        context.Features.Set<IHttpResponseContentFeature>(adapterFeature);

        context.Response.RegisterForDisposeAsync(adapterFeature);

        return adapterFeature;
    }
}
