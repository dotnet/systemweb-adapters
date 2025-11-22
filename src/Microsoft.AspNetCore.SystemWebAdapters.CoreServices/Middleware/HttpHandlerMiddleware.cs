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
        private IHttpHandler? _current;
        private IHttpHandler? _fromMetadata;

        public IHttpHandler? Previous { get; set; }

        public Endpoint? Endpoint
        {
            get;
            set
            {
                if (value is { })
                {
                    field = value;
                    _current = null;
                    _fromMetadata = value.Metadata.GetMetadata<IHandlerMetadata>()?.GetHandler(context);
                }
                else
                {
                    field = null;
                    _fromMetadata = null;
                }
            }
        }

        public IHttpHandler? Current
        {
            get => _current ?? _fromMetadata;
            set
            {
                Previous = _current;
                _current = value;

                if (value is { })
                {
                    Endpoint = null;
                }
            }
        }

        public bool IsEndpointHandler => _fromMetadata is { };
    }
}
