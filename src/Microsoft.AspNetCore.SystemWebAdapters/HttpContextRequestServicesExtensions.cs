// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web;

#if NETFRAMEWORK
/// <summary>
/// This is used instead of the standard IServiceScopeFactory to avoid taking a dependency on Microsoft.Extensions.DependencyInjection.Abstractions
/// </summary>
internal interface IServiceScopeFactoryProxy
{
    IServiceScopeProxy CreateScope();
}

/// <summary>
/// This is used instead of the standard IServiceScope to avoid taking a dependency on Microsoft.Extensions.DependencyInjection.Abstractions
/// </summary>
internal interface IServiceScopeProxy : IDisposable
{
    IServiceProvider ServiceProvider { get; }
}
#endif

[Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1510:Use ArgumentNullException throw helper", Justification = "<Pending>")]
public static class HttpContextRequestServicesExtensions
{
    public static IServiceProvider GetRequestServices(this HttpContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

#if NET
        return context.AsAspNetCore().RequestServices;
#elif NETSTANDARD
        throw new PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");
#else
        var sp = GetServiceProviderInternal(context.Items, out var scope);

        if (scope is { })
        {
            context.DisposeOnPipelineCompleted(scope);
        }

        return sp;
#endif
    }

    public static IServiceProvider GetRequestServices(this HttpContextBase context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

#if NET
        if (context is HttpContextWrapper wrapper)
        {
            return wrapper.InnerContext.AsAspNetCore().RequestServices;
        }

        return context.GetService(typeof(IServiceProvider)) as IServiceProvider ?? throw new InvalidOperationException("Could not get a service provider for the current request");
#elif NETSTANDARD
        throw new PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");
#else
        var sp = GetServiceProviderInternal(context.Items, out var scope);

        if (scope is { })
        {
            context.DisposeOnPipelineCompleted(scope);
        }

        return sp;
#endif
    }

#if NETFRAMEWORK
    private const string RequestServicesKey = "__RequestServices";

    private static IServiceProvider GetServiceProviderInternal(IDictionary items, out IServiceScopeProxy? scoped)
    {
        if (items[RequestServicesKey] is IServiceProvider services)
        {
            scoped = null;
            return services;
        }

        var scope = HttpRuntime.WebObjectActivator?.GetService(typeof(IServiceScopeFactoryProxy)) as IServiceScopeFactoryProxy;

        if (scope is null)
        {
            throw new InvalidOperationException("Could not retrieve service to get scoped services.");
        }

        scoped = scope.CreateScope();

        items[RequestServicesKey] = scoped.ServiceProvider;

        return scoped.ServiceProvider;
    }
#endif
}
