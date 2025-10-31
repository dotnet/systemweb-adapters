// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Owin;
using Owin;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed partial class IntegratedPipelineConfigureOptions(IOptions<OwinAppOptions> owinOptions, IServiceProvider sp) : IConfigureOptions<HttpApplicationOptions>
{
    public void Configure(HttpApplicationOptions options)
    {
        var stages = CreateStages(owinOptions.Value.Configure, sp);

        foreach (var stage in stages)
        {
            var appEvent = stage.Name switch
            {
                OwinConstants.StageAuthenticate => ApplicationEvent.AuthenticateRequest,
                OwinConstants.StagePostAuthenticate => ApplicationEvent.PostAuthenticateRequest,
                OwinConstants.StageAuthorize => ApplicationEvent.AuthorizeRequest,
                OwinConstants.StagePostAuthorize => ApplicationEvent.PostAuthorizeRequest,
                OwinConstants.StageResolveCache => ApplicationEvent.ResolveRequestCache,
                OwinConstants.StagePostResolveCache => ApplicationEvent.PostResolveRequestCache,
                OwinConstants.StageMapHandler => ApplicationEvent.MapRequestHandler,
                OwinConstants.StagePostMapHandler => ApplicationEvent.PostMapRequestHandler,
                OwinConstants.StageAcquireState => ApplicationEvent.AcquireRequestState,
                OwinConstants.StagePostAcquireState => ApplicationEvent.PostAcquireRequestState,
                OwinConstants.StagePreHandlerExecute => ApplicationEvent.PreRequestHandlerExecute,
                _ => default
            };

            if (appEvent is { } value)
            {
                options.RegisterEvent(value, stage.Next);
            }
        }
    }

    private sealed record OwinStage(string Name, RequestDelegate Next);

    private static IEnumerable<OwinStage> CreateStages(Action<IAppBuilder, IServiceProvider>? configure, IServiceProvider services)
    {
        if (configure is null)
        {
            return [];
        }

        StageBuilder? firstStage = null;

        Task DefaultApp(IDictionary<string, object> env)
        {
            if (!env.TryGetValue(OwinConstants.IntegratedPipelineCurrentStage, out var currentStage))
            {
                throw new InvalidOperationException("No current stage for OWIN pipeline");
            }

            if (env.TryGetValue(typeof(HttpContext).FullName!, out var obj) && obj is HttpContext httpContext)
            {
                return httpContext.JoinPipelineFork(currentStage);
            }

            throw new InvalidOperationException("No HttpContext");
        }

        var appFunc = OwinBuilder.Build(DefaultApp, (builder, sp) =>
        {
            EnableIntegratedPipeline(builder, stage => firstStage = stage, DefaultApp, sp.GetRequiredService<ILogger<OwinStage>>());
            configure(builder, services);
        }, services);

        if (firstStage is null)
        {
            throw new InvalidOperationException("Did not have a stage");
        }

        return [.. GetStages(firstStage, appFunc, services)];
    }

    private static IEnumerable<OwinStage> GetStages(StageBuilder? stage, AppFunc appFunc, IServiceProvider services)
    {
        while (stage is not null)
        {
            var app = new ApplicationBuilder(services);

            var name = stage.Name;
            var entrypoint = stage.EntryPoint ?? appFunc;

            app.Use((ctx, next) => ctx.RunForkedPipelineAsync(name, next));
            app.UseOwin(setup => setup.Use<StageMiddleware>(name, entrypoint));

            yield return new(name, app.Build());
            stage = stage.NextStage;
        }
    }

    private sealed class StageMiddleware(OwinMiddleware next, string name, AppFunc entrypoint) : OwinMiddleware(next)
    {
        public override async Task Invoke(IOwinContext context)
        {
            context.Set(OwinConstants.IntegratedPipelineCurrentStage, name);
            await entrypoint(context.Environment);
        }
    }

    private sealed class StageBuilder
    {
        public required string Name { get; set; }

        public StageBuilder? NextStage { get; set; }

        public AppFunc? EntryPoint { get; set; }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Integrated Pipeline stage '{StageName}' added out of order before stage '{CurrentStageName}'. This stage will be ignored and middleware may run earlier than expected.")]
    private static partial void LogOutOfOrder(ILogger logger, string stageName, string currentStageName);

    private static void EnableIntegratedPipeline(IAppBuilder app, Action<StageBuilder> onStageCreated, AppFunc exitPoint, ILogger logger)
    {
        var stage = new StageBuilder { Name = "PreHandlerExecute" };

        onStageCreated(stage);

        app.Properties[OwinConstants.IntegratedPipelineStageMarker] = (IAppBuilder builder, string name) =>
        {
            app.Use((AppFunc next) =>
            {
                if (string.Equals(name, stage.Name, StringComparison.OrdinalIgnoreCase))
                {
                    // no decoupling needed when pipeline is already split at this name
                    return next;
                }

                if (!VerifyStageOrder(name, stage.Name))
                {
                    LogOutOfOrder(logger, name, stage.Name);
                    return next;
                }

                stage.EntryPoint = next;
                stage = new()
                {
                    Name = name,
                    NextStage = stage,
                };

                onStageCreated(stage);

                return exitPoint;
            });
        };
    }

    private static readonly string[] StageNames =
        [
            OwinConstants.StageAuthenticate,
            OwinConstants.StagePostAuthenticate,
            OwinConstants.StageAuthorize,
            OwinConstants.StagePostAuthorize,
            OwinConstants.StageResolveCache,
            OwinConstants.StagePostResolveCache,
            OwinConstants.StageMapHandler,
            OwinConstants.StagePostMapHandler,
            OwinConstants.StageAcquireState,
            OwinConstants.StagePostAcquireState,
            OwinConstants.StagePreHandlerExecute,
        ];

    internal static bool VerifyStageOrder(string stage1, string stage2)
    {
        var stage1Index = Array.IndexOf(StageNames, stage1);
        var stage2Index = Array.IndexOf(StageNames, stage2);

        if (stage1Index == -1 || stage2Index == -1)
        {
            return false;
        }
        return stage1Index < stage2Index;
    }
}
