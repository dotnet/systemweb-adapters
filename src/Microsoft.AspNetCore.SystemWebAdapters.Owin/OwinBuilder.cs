// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Owin.Builder;
using Microsoft.Owin.BuilderProperties;
using Owin;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static class OwinBuilder
{
    public static AppFunc Build(AppFunc defaultApp, Action<IAppBuilder, IServiceProvider> configure, IServiceProvider services)
    {
        var owinAppBuilder = new AppBuilder();
        var lifetime = services.GetRequiredService<IHostApplicationLifetime>();
        var env = services.GetRequiredService<IWebHostEnvironment>();
        var owinAppProperties = new AppProperties(owinAppBuilder.Properties);

        owinAppProperties.OnAppDisposing = lifetime.ApplicationStopping;

        owinAppProperties.DefaultApp = defaultApp;

        owinAppProperties.AppName = env.ApplicationName;

        configure(owinAppBuilder, services);

        return owinAppBuilder.Build();
    }
}
