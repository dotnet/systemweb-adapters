// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.Web;

public static class SystemWebAdapterExtensions
{
    public static IHostApplicationBuilder ConfigureHttpApplication(this IHostApplicationBuilder builder, Action<HttpApplicationHostOptions> configure)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        builder.Services.AddOptions<HttpApplicationHostOptions>()
            .Configure(configure);

        return builder;
    }

    public static ISystemWebAdapterBuilder AddSystemAdapters(this IServiceCollection services)
       => new SystemWebAdapterBuilder(services);
}
