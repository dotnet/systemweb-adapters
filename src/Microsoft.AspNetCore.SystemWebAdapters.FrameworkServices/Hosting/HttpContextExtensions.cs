// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters.Hosting;

public static class HttpContextExtensions
{
    private static object _scopedKey = new();

    public static ISystemWebAdapterBuilder AddSystemAdapters(this IServiceCollection services)
       => new SystemWebAdapterBuilder(services);

    public static IServiceProvider GetScopedServiceProvider(this HttpContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Items[_scopedKey] is IServiceScope existingScope)
        {
            return existingScope.ServiceProvider;
        }

        var scope = HttpApplicationHost.Current.Services.CreateScope();

        context.Items[_scopedKey] = scope;

        context.AddOnRequestCompleted(static context =>
        {
            if (context.Items[_scopedKey] is IServiceScope scope)
            {
                scope.Dispose();
            }
        });

        return scope.ServiceProvider;
    }
}
