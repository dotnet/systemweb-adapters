// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.Middleware;

internal sealed class HttpHandlerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContextCore context)
    {
        var feature = new HttpHandlerFeature(context);

        context.Features.Set<IHttpHandlerFeature>(feature);
        context.Features.Set<IEndpointFeature>(feature);

        try
        {
            await next(context);
        }
        finally
        {
            context.Features.Set<IHttpHandlerFeature>(null);
            context.Features.Set<IEndpointFeature>(null);
        }
    }

    private sealed partial class HttpHandlerFeature(HttpContextCore context) : IHttpHandlerFeature, IEndpointFeature
    {
        public IHttpHandler? Previous { get; set; }

        public Endpoint? Endpoint
        {
            get;
            set
            {
                field = value;

                if (value is { })
                {
                    Current = null;
                }
            }
        }

        public IHttpHandler? Current
        {
            get
            {
                if (field is { } handler)
                {
                    IsEndpointHandler = false;
                    return handler;
                }

                if (Endpoint?.Metadata.GetMetadata<IHandlerMetadata>() is { } metadata)
                {
                    IsEndpointHandler = true;
                    return metadata.GetHandler(context);
                }

                return null;
            }
            set
            {
                Previous = field;
                field = value;

                if (value is { })
                {
                    IsEndpointHandler = true;
                    Endpoint = null;
                }
            }
        }

        public bool IsEndpointHandler { get; set; }
    }
}
