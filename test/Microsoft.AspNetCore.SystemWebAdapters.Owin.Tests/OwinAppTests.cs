// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Owin;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

[Collection(nameof(SelfHostedTests))]
public class OwinAppTests
{
    [Fact]
    public async Task EventIntegration()
    {
        // Arrange
        var result = new Result();
        var builder = WebApplication.CreateSlimBuilder();

        builder.Logging.AddDebug();
        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);

        builder.Services.AddSingleton<IStartupFilter>(new ResultWriterStartupFilter(result));
        builder.Services.AddSystemWebAdapters()
            .AddOwinApp(app =>
            {
                app.Use<TestOwinMiddleware>("Owin1a");
                app.Use<TestOwinMiddleware>("Owin1b");
                app.UseStageMarker(PipelineStage.Authenticate);
                app.Use<TestOwinMiddleware>("Owin2a");
                app.Use<TestOwinMiddleware>("Owin2b");
                app.UseStageMarker(PipelineStage.Authorize);

                // This will fire in the PreRequestHandler stage which is at the end of the SystemWebAdapters middleware
                app.Use<TestOwinMiddleware>("Owin3");
            });

        var app = builder.Build();

        app.UseMiddleware<TestMiddleware>("AuthN");
        app.UseAuthenticationEvents();
        app.UseMiddleware<TestMiddleware>("AuthZ");
        app.UseAuthorizationEvents();
        app.UseSystemWebAdapters();
        app.UseMiddleware<TestMiddleware>("Endpoints");
        app.MapGet("/", () => TypedResults.Ok());

        await app.StartAsync();

        // Act
        using var client = app.GetTestClient();
        var s = app.GetTestServer();
        using var response = await app.GetTestClient().GetAsync(new Uri("/", UriKind.Relative));

        // Assert
        Assert.Equal([
            "Before: AuthN",
            "Before: Owin1a",
            "Before: Owin1b",
            "Before: AuthZ",
            "Before: Owin2a",
            "Before: Owin2b",
            "Before: Owin3",
            "Before: Endpoints",
            "After: Endpoints",
            "After: AuthZ",
            "After: AuthN",
            "After: Owin3",
            "After: Owin2b",
            "After: Owin2a",
            "After: Owin1b",
            "After: Owin1a"], [.. result]);
    }

    [Fact]
    public async Task UseOwinTests()
    {
        // Arrange
        var result = new Result();
        var builder = WebApplication.CreateSlimBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);

        builder.Services.AddSingleton<IStartupFilter>(new ResultWriterStartupFilter(result));

        var app = builder.Build();

        app.UseOwin((app, services) =>
        {
            app.Use<TestOwinMiddleware>("a");
            app.Use<TestOwinMiddleware>("b");
        });

        app.UseMiddleware<TestMiddleware>("c");

        app.UseOwin(app =>
        {
            app.Use<TestOwinMiddleware>("d");
            app.Use<TestOwinMiddleware>("e");
        });

        app.UseOwin(app =>
        {
            app.Use<WriteOutputMiddleware>();
        });

        app.Run(ctx => Task.CompletedTask);

        await app.StartAsync();

        // Act
        using var client = app.GetTestClient();
        var s = app.GetTestServer();
        var response = await app.GetTestClient().GetStringAsync(new Uri("/", UriKind.Relative));

        // Assert
        Assert.Equal("Hello", response);
        Assert.Equal([
            "Before: a",
            "Before: b",
            "Before: c",
            "Before: d",
            "Before: e",
            "After: e",
            "After: d",
            "After: c",
            "After: b",
            "After: a",
            ], result);
    }

    private sealed class TestMiddleware(RequestDelegate next, string text)
    {
        public async Task InvokeAsync(HttpContextCore context)
        {
            context.Features.GetRequiredFeature<Result>().Add($"Before: {text}");
            await next(context);
            context.Features.GetRequiredFeature<Result>().Add($"After: {text}");
        }
    }

    private sealed class TestOwinMiddleware(OwinMiddleware next, string text) : OwinMiddleware(next)
    {
        public override async Task Invoke(IOwinContext context)
        {
            context.GetHttpContext().Features.GetRequiredFeature<Result>().Add($"Before: {text}");
            await Next.Invoke(context);
            context.GetHttpContext().Features.GetRequiredFeature<Result>().Add($"After: {text}");
        }
    }

    private sealed class WriteOutputMiddleware(OwinMiddleware next) : OwinMiddleware(next)
    {
        public override Task Invoke(IOwinContext context)
        {
            context.Response.Body.Write("Hello"u8);
            return Next.Invoke(context);
        }
    }

    private sealed class ResultWriterStartupFilter(Result result) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder =>
            {
                builder.Use(async (ctx, next) =>
                {
                    ctx.Features.Set(result);
                    await next(ctx);
                });
                next(builder);
            };
    }

    private sealed class Result : List<string>
    {
    }
}
