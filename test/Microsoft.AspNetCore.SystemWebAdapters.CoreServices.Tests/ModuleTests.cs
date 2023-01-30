// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class ModuleTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EventsRaisedAsExpectedAsync(bool endEarly)
    {
        // Arrange
        var tracker = new EventTracker();
        var pipeline = CreatePipeline(services =>
        {
            services.AddLogging();
            services.AddSingleton<EventTracker>(tracker);
            services.AddSystemWebAdapters()
                .AddHttpModule<MyModule1>()
                .AddHttpModule<MyModule2>()
                .AddHttpModule<MyModule3>();
        }, app =>
        {
            app.UseRaiseAuthenticationEvents();
            app.UseRaiseAuthorizationEvents();
            app.UseSystemWebAdapters();
        });

        var context = new DefaultHttpContext();

        if (endEarly)
        {
            context.Items["end"] = true;
        }

        // Act
        await pipeline(context);

        // Assert
        if (endEarly)
        {
            Assert.Collection(tracker,
                r => Assert.Equal("MyModule1.BeginRequest", r),
                r => Assert.Equal("MyModule2.BeginRequest", r),
                r => Assert.Equal("MyModule3.BeginRequest", r),
                r => Assert.Equal("MyModule2.EndRequest", r),
                r => Assert.Equal("MyModule3.EndRequest", r));
        }
        else
        {
            Assert.Collection(tracker,
                r => Assert.Equal("MyModule1.BeginRequest", r),
                r => Assert.Equal("MyModule2.BeginRequest", r),
                r => Assert.Equal("MyModule3.BeginRequest", r),
                r => Assert.Equal("MyModule1.PostAuthenticateRequest", r),
                r => Assert.Equal("MyModule2.PostAuthenticateRequest", r),
                r => Assert.Equal("MyModule1.MapRequestHandler", r),
                r => Assert.Equal("MyModule2.MapRequestHandler", r),
                r => Assert.Equal("MyModule2.EndRequest", r),
                r => Assert.Equal("MyModule3.EndRequest", r));
        }
    }

    private static RequestDelegate CreatePipeline(Action<IServiceCollection> serviceConfigure, Action<IApplicationBuilder> configure)
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

        return ((IApplicationBuilder)app).Build();
    }

    class EventTracker : IReadOnlyList<string>
    {
        private readonly List<string> _events = new();

        public string this[int index] => _events[index];

        public int Count => _events.Count;

        public void Add(Type type, string name) => _events.Add($"{type.Name}.{name}");

        public IEnumerator<string> GetEnumerator() => _events.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    class MyModule1 : System.Web.IHttpModule
    {
        private readonly EventTracker _tracker;

        public MyModule1(EventTracker tracker)
        {
            _tracker = tracker;
        }

        public void Dispose()
        {
        }

        public void Init(System.Web.HttpApplication application)
        {
            application.BeginRequest += (s, e) =>
            {
                var context = ((System.Web.HttpApplication)s).Context;
                _tracker.Add(typeof(MyModule1), nameof(application.BeginRequest));
            };
            application.PostAuthenticateRequest += (s, e) =>
            {
                var context = ((System.Web.HttpApplication)s).Context;
                _tracker.Add(typeof(MyModule1), nameof(application.PostAuthenticateRequest));
            };
            application.MapRequestHandler += (s, e) =>
            {
                var context = ((System.Web.HttpApplication)s).Context;
                _tracker.Add(typeof(MyModule1), nameof(application.MapRequestHandler));
            };
        }
    }
    class MyModule2 : System.Web.IHttpModule
    {
        private readonly EventTracker _tracker;

        public MyModule2(EventTracker tracker)
        {
            _tracker = tracker;
        }

        public void Dispose()
        {
        }

        public void Init(System.Web.HttpApplication application)
        {
            application.BeginRequest += (s, e) =>
            {
                var context = ((System.Web.HttpApplication)s).Context;
                _tracker.Add(typeof(MyModule2), nameof(application.BeginRequest));
            };
            application.PostAuthenticateRequest += (s, e) =>
            {
                var context = ((System.Web.HttpApplication)s).Context;
                _tracker.Add(typeof(MyModule2), nameof(application.PostAuthenticateRequest));
            };
            application.EndRequest += (s, e) =>
            {
                var context = ((System.Web.HttpApplication)s).Context;
                _tracker.Add(typeof(MyModule2), nameof(application.EndRequest));
            };
            application.MapRequestHandler += (s, e) =>
            {
                var context = ((System.Web.HttpApplication)s).Context;
                _tracker.Add(typeof(MyModule2), nameof(application.MapRequestHandler));
            };
        }
    }
    class MyModule3 : System.Web.IHttpModule
    {
        private readonly EventTracker _tracker;

        public MyModule3(EventTracker tracker)
        {
            _tracker = tracker;
        }

        public void Dispose()
        {
        }
        public void Init(System.Web.HttpApplication application)
        {
            application.BeginRequest += (s, e) =>
            {
                var context = ((System.Web.HttpApplication)s).Context;
                _tracker.Add(typeof(MyModule3), nameof(application.BeginRequest));
                if (context.Items.Contains("end"))
                {
                    application.CompleteRequest();
                }
            };
            application.EndRequest += (s, e) =>
            {
                var context = ((System.Web.HttpApplication)s).Context;
                _tracker.Add(typeof(MyModule3), nameof(application.EndRequest));
            };
        }
    }
}
