// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class SystemWebAdapterModule : IHttpModule
{
    private IServiceScope? _scope;

    public void Dispose()
    {
        _scope?.Dispose();
    }

    public void Init(HttpApplication context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.Application.EnsureSystemWebAdapterBuilderBuilt();

        var serviceProvider = HttpApplicationHost.Current.Services;

        if (serviceProvider is not null)
        {
            _scope = serviceProvider.CreateScope();
            foreach (var module in _scope.ServiceProvider.GetRequiredService<IEnumerable<IHttpModule>>())
            {
                module.Init(context);
            }
        }
    }
}
