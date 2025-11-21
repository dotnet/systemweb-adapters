// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CA1305 // Specify IFormatProvider
#pragma warning disable CA1307 // Specify StringComparison
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
#pragma warning disable CA2234 // Pass system uri objects instead of strings

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests;

[Collection(nameof(SelfHostedTests))]
public class HttpHandlerTests
{
    [Fact]
    public async Task MiddlewareSetsFeatures()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();

        IHttpHandlerFeature? httpHandlerFeature = null;
        IEndpointFeature? endpointFeature = null;

        app.Map("/", context =>
        {
            httpHandlerFeature = context.Features.Get<IHttpHandlerFeature>();
            endpointFeature = context.Features.Get<IEndpointFeature>();

            return Task.CompletedTask;
        });

        await app.StartAsync();

        var response = await app.GetTestClient().GetAsync("/");
        var _ = await response.Content.ReadAsStringAsync();

        Assert.NotNull(httpHandlerFeature);
        Assert.NotNull(endpointFeature);
        Assert.Same(httpHandlerFeature, endpointFeature);

        await app.StopAsync();
    }

    [Fact]
    public async Task NonEndpointHandlersExecuteThroughConditionalBranch()
    {
        const string expectedOutput = "Handler executed";
        var handler = new TestHttpHandler(expectedOutput);

        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);

        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.Use(async (ctx, next) =>
        {
            ctx.AsSystemWeb().Handler = handler;
            await next(ctx);
        });

        app.UseSystemWebAdapters();

        app.Map("/", context => Task.CompletedTask);

        await app.StartAsync();

        var response = await app.GetTestClient().GetAsync("/");

        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(expectedOutput, content);

        await app.StopAsync();
    }

    [Fact]
    public async Task EndpointHandlersSkipConditionalBranch()
    {
        const string expectedOutput = "Endpoint handler executed";

        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);

        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();

        app.MapHttpHandler("/", new TestHttpHandler(expectedOutput));

        await app.StartAsync();

        var response = await app.GetTestClient().GetAsync("/");

        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(expectedOutput, content);

        await app.StopAsync();
    }

    [Fact]
    public async Task NonEndpointHandlerShortCircuitsPipeline()
    {
        const string expectedOutput = "NonEndpoint handler executed";
        var handler = new TestHttpHandler(expectedOutput);

        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);

        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.Use((ctx, next) =>
        {
            ctx.AsSystemWeb().Handler = handler;
            return next(ctx);
        });

        app.UseSystemWebAdapters();

        app.Run(_ => throw new InvalidOperationException("Middleware shouldn't be run in this test"));

        app.Map("/", context => throw new InvalidOperationException("Endpoint shouldn't be reached"));

        await app.StartAsync();

        var response = await app.GetTestClient().GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(expectedOutput, content);

        await app.StopAsync();
    }

    [Theory]
    [InlineData(true, "SimpleHandler")]
    [InlineData(false, "SimpleHandler")]
    public async Task MapHttpHandlerWorks(bool useType, string expectedOutput)
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();

        if (useType)
        {
            app.MapHttpHandler<SimpleHandler>("/test");
        }
        else
        {
            app.MapHttpHandler("/test", new SimpleHandler());
        }

        await app.StartAsync();

        var response = await app.GetTestClient().GetAsync("/test");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedOutput, await response.Content.ReadAsStringAsync());

        await app.StopAsync();
    }

    [Theory]
    [InlineData(typeof(ReadOnlySessionHandler), "ReadOnly")]
    [InlineData(typeof(RequiredSessionHandler), "Required")]
    public async Task SessionHandlerGetsCorrectMetadata(Type handlerType, string expectedBehavior)
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);

        builder.Services.AddSession();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSystemWebAdapters()
            .AddWrappedAspNetCoreSession();

        await using var app = builder.Build();

        app.UseSession();
        app.UseSystemWebAdapters();

        app.MapHttpHandler("/session", handlerType);

        await app.StartAsync();

        var response = await app.GetTestClient().GetAsync("/session");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(expectedBehavior, content);

        await app.StopAsync();
    }

    [Theory]
    [InlineData("BufferResponseStream")]
    [InlineData("PreBufferRequestStream")]
    [InlineData("SetThreadCurrentPrincipal")]
    public async Task HandlerGetsMetadata(string metadataName)
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();
        app.MapHttpHandler<MetadataTestHandler>("/");

        await app.StartAsync();

        var response = await app.GetTestClient().GetAsync("/");

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(metadataName, content);

        await app.StopAsync();
    }

    [Theory]
    [InlineData(true, "SimpleHandler")]
    [InlineData(false, "SimpleHandler")]
    public async Task RunHttpHandlerWorks(bool useType, string expectedOutput)
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();

        if (useType)
        {
            app.RunHttpHandler<SimpleHandler>();
        }
        else
        {
            app.RunHttpHandler(new SimpleHandler());
        }

        await app.StartAsync();

        var response = await app.GetTestClient().GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedOutput, await response.Content.ReadAsStringAsync());

        await app.StopAsync();
    }

    [Fact]
    public async Task RunHttpHandlerExecutesCurrentHandler()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();
        app.Use(async (ctx, next) =>
        {
            var feature = ctx.Features.Get<Features.IHttpHandlerFeature>();
            if (feature is not null)
            {
                feature.Current = new SimpleHandler();
            }
            await next(ctx);
        });
        app.RunHttpHandler();

        await app.StartAsync();

        var response = await app.GetTestClient().GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("SimpleHandler", await response.Content.ReadAsStringAsync());

        await app.StopAsync();
    }

    [Fact]
    public async Task RunHttpHandlerThrowsWhenNoHandler()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();
        app.RunHttpHandler();

        await app.StartAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => app.GetTestClient().GetAsync("/"));
        Assert.Contains("No current HTTP handler was registered", exception.Message);

        await app.StopAsync();
    }

    [Theory]
    [InlineData(typeof(SynchronousHandler), "/sync", "Synchronous")]
    [InlineData(typeof(AsyncHandler), "/async", "Async")]
    [InlineData(typeof(TaskAsyncHandler), "/task", "TaskAsync")]
    public async Task HandlersExecute(Type handlerType, string path, string expectedOutput)
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();
        app.MapHttpHandler(path, handlerType);

        await app.StartAsync();

        var response = await app.GetTestClient().GetAsync(path);

        Assert.Equal(expectedOutput, await response.Content.ReadAsStringAsync());

        await app.StopAsync();
    }

    [Fact]
    public async Task ResponseEndStopsExecution()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();
        app.MapHttpHandler<ResponseEndHandler>("/end");

        await app.StartAsync();

        var response = await app.GetTestClient().GetAsync("/end");

        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Before", content);
        Assert.DoesNotContain("After", content);

        await app.StopAsync();
    }

    [Fact]
    public async Task HandlerSupportsDependencyInjection()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();
        builder.Services.AddSingleton<TestService>();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();
        app.MapHttpHandler<DIHandler>("/di");

        await app.StartAsync();

        var response = await app.GetTestClient().GetAsync("/di");

        Assert.Equal("DI:TestService", await response.Content.ReadAsStringAsync());

        await app.StopAsync();
    }

    [Fact]
    public async Task HandlerIsCreatedPerRequest()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();
        app.MapHttpHandler<CounterHandler>("/counter");

        await app.StartAsync();

        var result1 = await app.GetTestClient().GetStringAsync("/counter");
        var result2 = await app.GetTestClient().GetStringAsync("/counter");

        Assert.Equal("0", result1);
        Assert.Equal("1", result2);

        await app.StopAsync();
    }

    [Fact]
    public async Task MultipleHandlersCanBeMapped()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();
        app.MapHttpHandler<SimpleHandler>("/simple");
        app.MapHttpHandler<AsyncHandler>("/async");

        await app.StartAsync();

        var result1 = await app.GetTestClient().GetStringAsync("/simple");
        var result2 = await app.GetTestClient().GetStringAsync("/async");

        Assert.Equal("SimpleHandler", result1);
        Assert.Equal("Async", result2);

        await app.StopAsync();
    }

    [Fact]
    public async Task FeaturesAreClearedAfterRequest()
    {
        IHttpHandlerFeature? capturedHandlerFeature = null;
        IEndpointFeature? capturedEndpointFeature = null;

        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();

        app.Use(async (ctx, next) =>
        {
            capturedHandlerFeature = ctx.Features.Get<IHttpHandlerFeature>();
            capturedEndpointFeature = ctx.Features.Get<IEndpointFeature>();
            await next(ctx);
        });

        app.Map("/", context =>
        {
            Assert.NotNull(context.AsSystemWeb().AsAspNetCore().Features.Get<IHttpHandlerFeature>());
            Assert.NotNull(context.AsSystemWeb().AsAspNetCore().Features.Get<IEndpointFeature>());
            return Task.CompletedTask;
        });

        await app.StartAsync();

        await app.GetTestClient().GetAsync("/");

        Assert.NotNull(capturedHandlerFeature);
        Assert.NotNull(capturedEndpointFeature);

        await app.StopAsync();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task IsEndpointHandlerCorrectlySet(bool useMappedHandler, bool expectedIsEndpointHandler)
    {
        var handler = new TestHandler();

        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();

        bool isEndpointHandler = !expectedIsEndpointHandler;

        app.Use(async (ctx, next) =>
        {
            if (!useMappedHandler)
            {
                var feature = ctx.Features.GetRequiredFeature<IHttpHandlerFeature>();
                feature.Current = handler;
                isEndpointHandler = feature.IsEndpoint;
            }
            await next(ctx);
        });

        if (useMappedHandler)
        {
            app.Use(async (ctx, next) =>
            {
                await next(ctx);
                var feature = ctx.Features.GetRequiredFeature<IHttpHandlerFeature>();
                isEndpointHandler = feature.IsEndpoint;
            });
            app.MapHttpHandler("/", handler);
        }
        else
        {
            app.Map("/", _ => Task.CompletedTask);
        }

        await app.StartAsync();

        await app.GetTestClient().GetAsync("/");

        Assert.Equal(expectedIsEndpointHandler, isEndpointHandler);

        await app.StopAsync();
    }

    [Fact]
    public async Task CurrentReturnsEndpointHandler()
    {
        var handler = new TestHandler();

        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();

        IHttpHandler? current = null;
        bool isEndpointHandler = false;

        app.Use(async (ctx, next) =>
        {
            await next(ctx);
            var feature = ctx.Features.GetRequiredFeature<IHttpHandlerFeature>();
            current = feature.Current;
            isEndpointHandler = feature.IsEndpoint;
        });

        app.MapHttpHandler("/", handler);

        await app.StartAsync();

        await app.GetTestClient().GetAsync("/");

        Assert.Same(handler, current);
        Assert.True(isEndpointHandler);

        await app.StopAsync();
    }

    [Fact]
    public async Task CurrentReturnsMappedHandler()
    {
        var testHandler = new TestHandler();

        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();

        IHttpHandler? retrievedHandler = null;

        app.Use(async (ctx, next) =>
        {
            await next(ctx);
            var feature = ctx.Features.GetRequiredFeature<IHttpHandlerFeature>();
            retrievedHandler = feature.Current;
        });
        app.MapHttpHandler("/", testHandler);

        await app.StartAsync();

        await app.GetTestClient().GetAsync("/");

        Assert.NotNull(retrievedHandler);
        Assert.IsType<TestHandler>(retrievedHandler);

        await app.StopAsync();
    }

    [Fact]
    public async Task CurrentReturnsNullWhenNoHandler()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer(options => options.AllowSynchronousIO = true);
        builder.Services.AddSystemWebAdapters();

        await using var app = builder.Build();

        app.UseSystemWebAdapters();

        IHttpHandler? retrievedHandler = null;

        app.Map("/", context =>
        {
            var feature = context.AsSystemWeb().AsAspNetCore().Features.GetRequiredFeature<IHttpHandlerFeature>();
            retrievedHandler = feature.Current;
            return Task.CompletedTask;
        });

        await app.StartAsync();

        await app.GetTestClient().GetAsync("/");

        Assert.Null(retrievedHandler);

        await app.StopAsync();
    }

    private sealed class TestHttpHandler(string textToWrite) : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.Write(textToWrite);
        }
    }

    private sealed class SimpleHandler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Write("SimpleHandler");
        }
    }

    private sealed class SynchronousHandler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Write("Synchronous");
        }
    }

    private sealed class AsyncHandler : IHttpAsyncHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object? extraData)
        {
            var tcs = new TaskCompletionSource<bool>(extraData);
            context.Response.Write("Async");
            tcs.SetResult(true);
            var result = tcs.Task;
            if (cb != null)
            {
                result.ContinueWith(_ => cb(tcs.Task));
            }
            return tcs.Task;
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            ((Task)result).GetAwaiter().GetResult();
        }
    }

    private sealed class TaskAsyncHandler : HttpTaskAsyncHandler
    {
        public override Task ProcessRequestAsync(HttpContext context)
        {
            context.Response.Write("TaskAsync");
            return Task.CompletedTask;
        }
    }

    private sealed class ResponseEndHandler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Write("Before");
            context.Response.End();
            context.Response.Write("After");
        }
    }

    private sealed class ReadOnlySessionHandler : IHttpHandler, IReadOnlySessionState
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            var endpoint = context.AsAspNetCore().GetEndpoint();
            var sessionAttr = endpoint?.Metadata.GetMetadata<SessionAttribute>();
            context.Response.Write(sessionAttr?.SessionBehavior.ToString() ?? "None");
        }
    }

    private sealed class RequiredSessionHandler : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            var endpoint = context.AsAspNetCore().GetEndpoint();
            var sessionAttr = endpoint?.Metadata.GetMetadata<SessionAttribute>();
            context.Response.Write(sessionAttr?.SessionBehavior.ToString() ?? "None");
        }
    }

    private sealed class MetadataTestHandler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            var endpoint = context.AsAspNetCore().GetEndpoint();
            var metadata = endpoint?.Metadata;

            var list = new List<string>();

            if (metadata?.GetMetadata<BufferResponseStreamAttribute>() is { })
            {
                list.Add("BufferResponseStream");
            }

            if (metadata?.GetMetadata<PreBufferRequestStreamAttribute>() is { })
            {
                list.Add("PreBufferRequestStream");
            }

            if (metadata?.GetMetadata<SetThreadCurrentPrincipalAttribute>() is { })
            {
                list.Add("SetThreadCurrentPrincipal");
            }

            context.Response.Write(string.Join(",", list));
        }
    }

    private sealed class DIHandler : IHttpHandler
    {
        private readonly TestService _service;

        public DIHandler(TestService service)
        {
            _service = service;
        }

        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Write($"DI:{_service.Name}");
        }
    }

    private sealed class CounterHandler : IHttpHandler
    {
        private static int _counter;

        public CounterHandler()
        {
            CurrentCount = _counter++;
        }

        public bool IsReusable => true;

        public int CurrentCount { get; }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Write(CurrentCount.ToString());
        }
    }

    private sealed class TestHandler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Write("Test");
        }
    }

    private sealed class TestService
    {
        public string Name => "TestService";
    }
}
