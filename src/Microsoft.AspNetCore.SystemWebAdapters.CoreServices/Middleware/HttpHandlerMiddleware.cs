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

    /// <summary>
    /// An implementation of <see cref="IHttpHandlerFeature"/> that integrates with endpoint
    /// routing via <see cref="IEndpointFeature"/> so that either a <see cref="IHttpHandler"/>
    /// or an <see cref="Http.Endpoint"/> can be used to handle the request.
    /// 
    /// There is a special case when the <see cref="Http.Endpoint"/> is set that has <see cref="IHandlerMetadata"/> metadata.
    /// In that case the <see cref="IHttpHandler"/> is obtained from the metadata and used as the current handler, but will be
    /// handled by the endpoint routing system.
    /// </summary>
    private sealed partial class HttpHandlerFeature(HttpContextCore context) : IHttpHandlerFeature, IEndpointFeature
    {
        public IHttpHandler? Previous { get; set; }

        public Endpoint? Endpoint
        {
            get;
            set
            {
                if (value is { })
                {
                    Current = value.Metadata.GetMetadata<IHandlerMetadata>()?.GetHandler(context);
                }

                // Must set after setting Current to avoid clearing it
                field = value;
            }
        }

        public IHttpHandler? Current
        {
            get => field;
            set
            {
                Previous = field;
                field = value;

                if (value is { })
                {
                    Endpoint = null;
                }
            }
        }

        bool IHttpHandlerFeature.IsEndpoint => Endpoint is { };
    }
}
