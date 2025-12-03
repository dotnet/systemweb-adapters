// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

public static class HttpHandlerBuilderExtensions
{
    /// <summary>
    /// Runs <typeparamref name="THandler"/> as part of a pipeline and then returns.
    /// </summary>
    public static void RunHttpHandler<THandler>(this IApplicationBuilder app)
        where THandler : IHttpHandler
    {
        ArgumentNullException.ThrowIfNull(app);

        var factory = ActivatorUtilities.CreateFactory<THandler>([]);

        app.UseMiddleware<RequestEndThrowsMiddleware>();
        app.Run(ctx =>
        {
            var handler = factory(ctx.RequestServices, []);
            return handler.RunAsync(ctx);
        });
    }

    /// <summary>
    /// Runs the specified <paramref name="handler"/> as part of a pipeline and then returns.
    /// </summary>
    public static void RunHttpHandler(this IApplicationBuilder app, IHttpHandler handler)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(handler);

        app.UseMiddleware<RequestEndThrowsMiddleware>();
        app.Run(ctx => handler.RunAsync(ctx));
    }

    /// <summary>
    /// Invokes the current <see cref="IHttpHandler"/> registered in <see cref="System.Web.HttpContext.Handler"/>.
    /// </summary>
    public static void RunHttpHandler(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<RequestEndThrowsMiddleware>();
        app.Run(ctx =>
        {
            var handler = ctx.Features.GetRequiredFeature<IHttpHandlerFeature>().Current
                ?? throw new InvalidOperationException("No current HTTP handler was registered");

            return handler.RunAsync(ctx);
        });
    }

    /// <summary>
    /// Maps the specified <typeparamref name="THandler"/> HTTP handler to the given route <paramref name="pattern"/>.
    /// </summary>
    public static IEndpointConventionBuilder MapHttpHandler<THandler>(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern)
        where THandler : class, IHttpHandler
        => endpoints.MapHttpHandler(pattern, typeof(THandler));

    /// <summary>
    /// Maps the specified <paramref name="handlerType"/> HTTP handler to the given route <paramref name="pattern"/>.
    /// </summary>
    public static IEndpointConventionBuilder MapHttpHandler(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern, Type handlerType)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(handlerType);

        if (!handlerType.IsAssignableTo(typeof(IHttpHandler)))
        {
            throw new ArgumentException($"The type '{handlerType.FullName}' does not implement '{typeof(IHttpHandler).FullName}'", nameof(handlerType));
        }

        var app = endpoints.CreateApplicationBuilder();

        app.RunHttpHandler();

        var metadata = new HandlerTypeMetadata(handlerType);

        return endpoints.Map(pattern, app.Build())
            .WithMetadata([metadata, .. metadata.DefaultMetadata]);
    }

    /// <summary>
    /// Maps the specified <paramref name="handler"/> to the given route <paramref name="pattern"/>.
    /// </summary>
    public static IEndpointConventionBuilder MapHttpHandler(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern, IHttpHandler handler)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(pattern);

        var app = endpoints.CreateApplicationBuilder();

        app.RunHttpHandler();

        var metadata = new HandlerInstanceMetadata(handler);

        return endpoints.Map(pattern, app.Build())
            .WithMetadata([metadata, .. metadata.DefaultMetadata]);
    }

    private static Task RunAsync(this IHttpHandler handler, HttpContextCore context)
    {
        if (handler is HttpTaskAsyncHandler task)
        {
            return task.ProcessRequestAsync(context);
        }
        else if (handler is IHttpAsyncHandler asyncHandler)
        {
            return Task.Factory.FromAsync((cb, state) => asyncHandler.BeginProcessRequest(context, cb, state), asyncHandler.EndProcessRequest, null);
        }
        else
        {
            handler.ProcessRequest(context);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// By design, the System.Web adapters try not to throw when <see cref="HttpResponse.End"/> but rather tracks it in other ways.
    /// However, <see cref="IHttpHandler"/> was built with that expectation so we want to break execution at the point of <see cref="HttpResponse.End"/>.
    /// </summary>
    private sealed class RequestEndThrowsMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContextCore context)
        {
            var original = context.Features.GetRequiredFeature<IHttpResponseEndFeature>();
            var endThrowingFeature = new RequestEndThrowingFeature();

            context.Features.Set<IHttpResponseEndFeature>(endThrowingFeature);

            try
            {
                await next(context).ConfigureAwait(false);

                context.Features.Set<IHttpResponseEndFeature>(original);
            }
            catch (RequestEndException)
            {
                // Must be set before we call EndAsync()
                context.Features.Set<IHttpResponseEndFeature>(original);

                await original.EndAsync().ConfigureAwait(false);
            }
        }

        private sealed class RequestEndThrowingFeature : IHttpResponseEndFeature
        {
            public bool IsEnded { get; private set; }

            public Task EndAsync()
            {
                IsEnded = true;
                throw new RequestEndException();
            }
        }

        [SuppressMessage("Design", "CA1064:Exceptions should be public", Justification = "Implementation detail that isn't intended to be used externally")]
        private sealed class RequestEndException : Exception
        {
        }
    }

    private sealed class HandlerTypeMetadata(Type type) : HandlerMetadataBase(type)
    {
        private readonly ObjectFactory _objectFactory = ActivatorUtilities.CreateFactory(type, []);

        public override IHttpHandler GetHandler(HttpContextCore context)
            => (IHttpHandler)_objectFactory(context.RequestServices, []);
    }

    private sealed class HandlerInstanceMetadata(IHttpHandler handler) : HandlerMetadataBase(handler.GetType())
    {
        public override IHttpHandler GetHandler(HttpContextCore context) => handler;
    }

    private abstract class HandlerMetadataBase(Type type) : IHandlerMetadata
    {
        private static object BufferResponse = new BufferResponseStreamAttribute();
        private static object BufferRequest = new PreBufferRequestStreamAttribute();
        private static object Principal = new SetThreadCurrentPrincipalAttribute();

        private static object ReadOnlySession = new SessionAttribute { SessionBehavior = SessionStateBehavior.ReadOnly };
        private static object RequiredSession = new SessionAttribute { SessionBehavior = SessionStateBehavior.Required };

        public IEnumerable<object> DefaultMetadata
        {
            get
            {
                if (type.IsAssignableTo(typeof(IReadOnlySessionState)))
                {
                    yield return ReadOnlySession;
                }
                else if (type.IsAssignableTo(typeof(IRequiresSessionState)))
                {
                    yield return RequiredSession;
                }

                yield return Principal;
                yield return BufferRequest;
                yield return BufferResponse;
            }
        }

        public Type Type => type;

        public abstract IHttpHandler GetHandler(HttpContextCore context);
    }
}

internal interface IHandlerMetadata
{
    Type Type { get; }

    IHttpHandler GetHandler(HttpContextCore context);
}
