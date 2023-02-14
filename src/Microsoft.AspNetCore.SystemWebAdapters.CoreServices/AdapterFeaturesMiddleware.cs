// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = Constants.DisposeIsRegistered)]
internal sealed class AdapterFeaturesMiddleware
{
    private readonly RequestDelegate _next;

    public AdapterFeaturesMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContextCore context)
    {
        RegisterRequestFeatures(context);
        var feature = RegisterResponseFeatures(context);

        await _next(context);
        await feature.FlushAsync();
    }

    private static IHttpRequestInputStreamFeature RegisterRequestFeatures(HttpContextCore context)
    {
        var existing = context.Features.GetRequired<IHttpRequestFeature>();

        var inputStreamFeature = new HttpRequestInputStreamFeature(existing);

        context.Response.RegisterForDispose(inputStreamFeature);
        context.Features.Set<IHttpRequestFeature>(inputStreamFeature);
        context.Features.Set<IHttpRequestInputStreamFeature>(inputStreamFeature);
        context.Features.Set<IRequestBodyPipeFeature>(inputStreamFeature);

        return inputStreamFeature;
    }

    private static IHttpResponseBufferingFeature RegisterResponseFeatures(HttpContextCore context)
    {
        var responseBodyFeature = context.Features.GetRequired<IHttpResponseBodyFeature>();

        var adapterFeature = new HttpResponseAdapterFeature(responseBodyFeature);

        context.Features.Set<IHttpResponseBodyFeature>(adapterFeature);
        context.Features.Set<IHttpResponseBufferingFeature>(adapterFeature);
        context.Features.Set<IHttpResponseEndFeature>(adapterFeature);

        context.Response.RegisterForDisposeAsync(adapterFeature);

        return adapterFeature;
    }
}
