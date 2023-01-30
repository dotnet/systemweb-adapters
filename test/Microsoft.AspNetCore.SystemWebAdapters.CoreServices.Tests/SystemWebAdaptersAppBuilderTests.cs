// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class SystemWebAdaptersAppBuilderTests
{
    private static readonly ImmutableList<string> AllOrderedExpectedEvents = new[]
    {
        nameof(IHttpApplicationEventsFeature.RaiseBeginRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaiseAuthenticateRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostAuthenticateRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaiseAuthorizeRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostAuthorizeRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaiseResolveRequestCacheAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostResolveRequestCacheAsync),
        nameof(IHttpApplicationEventsFeature.RaiseMapRequestHandlerAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostMapRequestHandlerAsync),
        nameof(IHttpApplicationEventsFeature.RaiseSessionStart),
        nameof(IHttpApplicationEventsFeature.RaiseAcquireRequestStateAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostAcquireRequestStateAsync),
        nameof(IHttpApplicationEventsFeature.RaisePreRequestHandlerExecuteAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostRequestHandlerExecuteAsync),
        nameof(IHttpApplicationEventsFeature.RaiseSessionEnd),
        nameof(IHttpApplicationEventsFeature.RaiseReleaseRequestStateAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostReleaseRequestStateAsync),
        nameof(IHttpApplicationEventsFeature.RaiseUpdateRequestCacheAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostUpdateRequestCacheAsync),
        nameof(IHttpApplicationEventsFeature.RaiseLogRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostLogRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaiseEndRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaisePreSendRequestHeaders),
        nameof(IHttpApplicationEventsFeature.RaiseRequestCompletedAsync),
    }.ToImmutableList();

    private static readonly ImmutableList<string> AuthenticationEvents = new[]
    {
        nameof(IHttpApplicationEventsFeature.RaiseAuthenticateRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostAuthenticateRequestAsync),
    }.ToImmutableList();

    private static readonly ImmutableList<string> AuthorizationEvents = new[]
    {
        nameof(IHttpApplicationEventsFeature.RaiseAuthorizeRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostAuthorizeRequestAsync),
    }.ToImmutableList();

    [InlineData(true, true, TestSessionState.None)]
    [InlineData(true, true, TestSessionState.New)]
    [InlineData(true, true, TestSessionState.Exisiting)]
    [InlineData(true, true, TestSessionState.Abandoned)]
    [InlineData(true, false, TestSessionState.None)]
    [InlineData(false, true, TestSessionState.None)]
    [InlineData(false, false, TestSessionState.None)]
    [Theory]
    public async Task EventRaising(bool useAuthentication, bool useAuthorization, TestSessionState state)
    {
        // Arrange
        var events = new Mock<IHttpApplicationEventsFeature>();
        var expected = AllOrderedExpectedEvents;
        var attribute = new SessionAttribute();
        var session = new Mock<ISessionState>();

        if (state is TestSessionState.New)
        {
            session.Setup(s => s.IsNewSession).Returns(true);
        }
        else
        {
            expected = expected.Remove(nameof(IHttpApplicationEventsFeature.RaiseSessionStart));
        }

        if (state is TestSessionState.Abandoned)
        {
            session.Setup(s => s.IsAbandoned).Returns(true);
        }
        else
        {
            expected = expected.Remove(nameof(IHttpApplicationEventsFeature.RaiseSessionEnd));
        }

        var sessionManager = new Mock<ISessionManager>();
        sessionManager.Setup(s => s.CreateAsync(It.IsAny<HttpContextCore>(), attribute)).ReturnsAsync(session.Object);

        var (services, pipeline) = CreatePipeline(services =>
        {
            services.AddLogging();
            services.AddSystemWebAdapters()
                .AddHttpModule<TestModule>();
            services.AddTransient<ISessionManager>(_ => sessionManager.Object);
        }, app =>
        {
            if (useAuthentication)
            {
                app.UseRaiseAuthenticationEvents();
            }

            if (useAuthorization)
            {
                app.UseRaiseAuthorizationEvents();
            }

            app.UseSystemWebAdapters();
        });

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = services;
        httpContext.Features.Set(events.Object);
        httpContext.SetEndpoint(new Endpoint(null, new EndpointMetadataCollection(attribute), null));
        var responseFeature = new TestHttpResponseFeature();
        httpContext.Features.Set<IHttpResponseFeature>(responseFeature);

        if (!useAuthentication)
        {
            expected = expected.RemoveRange(AuthenticationEvents);
        }

        if (!useAuthorization)
        {
            expected = expected.RemoveRange(AuthorizationEvents);
        }

        // Act
        await pipeline(httpContext);
        await responseFeature.Finalize();

        // Assert
        var names = events.Invocations.Select(i => i.Method.Name).ToImmutableList();
        //Assert.Equal(expected.Count, names.Count);
        Assert.Equal(expected, names);
    }

    private class TestHttpResponseFeature : HttpResponseFeature
    {
        private readonly List<Func<Task>> _onCompleted = new();
        private readonly List<Func<Task>> _onStarting = new();

        public override void OnCompleted(Func<object, Task> callback, object state)
            => _onCompleted.Add(() => callback(state));

        public override void OnStarting(Func<object, Task> callback, object state)
            => _onStarting.Add(() => callback(state));

        public async Task Finalize()
        {
            foreach (var onStarting in _onStarting)
            {
                await onStarting();
            }

            foreach (var onCompleted in _onCompleted)
            {
                await onCompleted();
            }
        }
    }

    [Fact]
    public void TestPipeline()
    {
        // Arrange
        var expected = new[]
        {
            typeof(SetHttpContextTimestampMiddleware).FullName,
            typeof(SetHttpApplicationMiddleware).FullName,
            typeof(HttpApplicationExtensions.RaiseAuthenticateRequest).FullName,
            typeof(HttpApplicationExtensions.RaiseAuthorizeRequest).FullName,
            typeof(EndRequestShortCircuitMiddleware).FullName,
            typeof(HttpApplicationMiddleEventsMiddleware).FullName,
            typeof(SessionMiddleware).FullName,
            typeof(SetDefaultResponseHeadersMiddleware).FullName,
            typeof(PreBufferRequestStreamMiddleware).FullName,
            typeof(BufferResponseStreamMiddleware).FullName,
            typeof(SingleThreadedRequestMiddleware).FullName,
            typeof(CurrentPrincipalMiddleware).FullName,
            typeof(HttpApplicationEventsHandlerMiddleware).FullName,
            "Microsoft.AspNetCore.Builder.ApplicationBuilder+<>c",
        };

        var httpContext = new DefaultHttpContext();
        var events = new Mock<IHttpApplicationEventsFeature>();

        var (_, pipeline) = CreatePipeline(services =>
        {
            services.AddLogging();
            services.AddOptions();
            services.AddSystemWebAdapters()
                .AddHttpModule<TestModule>();
        }, builder =>
        {
            builder.UseRaiseAuthenticationEvents();
            builder.UseRaiseAuthorizationEvents();
            builder.UseSystemWebAdapters();
        });

        // Act
        var targets = GetPipelineTargets(pipeline).ToArray();

        // Assert
        Assert.Equal(expected, targets);
    }

    private sealed class TestModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication application)
        {
        }
    }

    public enum TestSessionState
    {
        None,
        New,
        Exisiting,
        Abandoned
    }

    private static (IServiceProvider, RequestDelegate) CreatePipeline(Action<IServiceCollection> serviceConfigure, Action<IApplicationBuilder> configure)
    {
        var serviceBuilder = new ServiceCollection();

        serviceConfigure(serviceBuilder);

        var services = serviceBuilder.BuildServiceProvider();
        var app = new ApplicationBuilder(services);
        var startupFilters = services.GetService<IEnumerable<IStartupFilter>>();

        if (startupFilters is not null)
        {
            foreach (var filter in startupFilters.Reverse())
            {
                configure = filter.Configure(configure);
            }
        }

        configure(app);

        return (services, ((IApplicationBuilder)app).Build());
    }

    private static IEnumerable<string> GetPipelineTargets(RequestDelegate del)
    {
        Delegate? d = del;

        while (d is { Target: { } target })
        {
            yield return target.ToString() ?? "--Unknown--";

            d = GetNext(target.GetType()) is { } field && field.GetValue(target) is Delegate next ? next : null;
        }

        // Look for any field at any level that is of type RequestDelegate
        static FieldInfo? GetNext(Type type)
        {
            var t = type;

            while (t is not null)
            {
                var field = t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(f => f.FieldType == typeof(RequestDelegate));

                if (field is not null)
                {
                    return field;
                }

                t = t.BaseType;
            }

            return null;
        }
    }
}
