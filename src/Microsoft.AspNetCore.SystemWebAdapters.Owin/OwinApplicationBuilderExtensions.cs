// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Owin.Builder;
using Owin;

namespace Microsoft.AspNetCore.Builder;

public static class OwinApplicationBuilderExtensions
{
    /// <summary>
    /// Adds an OWIN middleware pipeline to the ASP.NET Core pipeline.
    /// </summary>
    public static IApplicationBuilder UseOwin(this IApplicationBuilder app, Action<IAppBuilder, IServiceProvider> configuration)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(configuration);

        return app.UseOwin(setup => setup(next => OwinBuilder.Build(next, configuration, app.ApplicationServices)));
    }

    /// <summary>
    /// Adds an OWIN middleware pipeline to the ASP.NET Core pipeline.
    /// </summary>
    public static IApplicationBuilder UseOwin(this IApplicationBuilder app, Action<IAppBuilder> configuration)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(configuration);

        return app.UseOwin(setup => setup(next => OwinBuilder.Build(next, (app, _) => configuration(app), app.ApplicationServices)));
    }

}
