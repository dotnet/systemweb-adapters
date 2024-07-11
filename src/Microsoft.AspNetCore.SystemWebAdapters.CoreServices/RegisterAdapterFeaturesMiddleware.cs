// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace Microsoft.Extensions.DependencyInjection;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = Constants.DisposeIsRegistered)]
internal sealed class RegisterAdapterFeaturesMiddleware
{
    private readonly RequestDelegate _next;

    public RegisterAdapterFeaturesMiddleware(RequestDelegate next)
        => _next = next;

    public async Task InvokeAsync(HttpContextCore context)
    {
        using (RegisterRequestFeatures(context))
        using (RegisterResponseFeatures(context))
        {
            try
            {
                await _next(context);
            }
            finally
            {
                await context.Features.GetRequiredFeature<IHttpResponseBufferingFeature>().FlushAsync();
            }
        }
    }

    private static DelegateDisposable RegisterRequestFeatures(HttpContextCore context)
    {
        var existing = context.Features.GetRequiredFeature<IHttpRequestFeature>();
        var existingPipe = context.Features.Get<IRequestBodyPipeFeature>();

        var inputStreamFeature = new HttpRequestInputStreamFeature(existing);

        context.Response.RegisterForDispose(inputStreamFeature);
        context.Features.Set<IHttpRequestFeature>(inputStreamFeature);
        context.Features.Set<IHttpRequestInputStreamFeature>(inputStreamFeature);
        context.Features.Set<IRequestBodyPipeFeature>(inputStreamFeature);
        context.Features.Set<IHttpRequestPathFeature>(inputStreamFeature);

        return new DelegateDisposable(() =>
        {
            context.Features.Set<IHttpRequestFeature>(existing);
            context.Features.Set<IRequestBodyPipeFeature>(existingPipe);
            context.Features.Set<IHttpRequestInputStreamFeature>(null);
            context.Features.Set<IHttpRequestPathFeature>(null);
        });
    }

    private static DelegateDisposable RegisterResponseFeatures(HttpContextCore context)
    {
        var responseBodyFeature = context.Features.GetRequiredFeature<IHttpResponseBodyFeature>();

        var adapterFeature = new HttpResponseAdapterFeature(responseBodyFeature);

        context.Features.Set<IHttpResponseBodyFeature>(adapterFeature);
        context.Features.Set<IHttpResponseBufferingFeature>(adapterFeature);
        context.Features.Set<IHttpResponseEndFeature>(adapterFeature);
        context.Features.Set<IHttpResponseContentFeature>(adapterFeature);

        context.Response.RegisterForDisposeAsync(adapterFeature);

        return new DelegateDisposable(() =>
        {
            context.Features.Set<IHttpResponseBodyFeature>(responseBodyFeature);
            context.Features.Set<IHttpResponseBufferingFeature>(null);
            context.Features.Set<IHttpResponseEndFeature>(null);
            context.Features.Set<IHttpResponseContentFeature>(null);
        });
    }

    private sealed class DelegateDisposable : IDisposable
    {
        private readonly Action _action;

        public DelegateDisposable(Action action) => _action = action;

        public void Dispose() => _action();
    }
}
