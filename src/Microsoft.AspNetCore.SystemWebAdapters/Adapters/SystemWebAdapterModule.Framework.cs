// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class SystemWebAdapterModule : IHttpModule
{
    private IEnumerable<IHttpModule>? _modules;

    public void Dispose()
    {
        if (_modules != null)
        {
            foreach (var module in _modules)
            {
                module.Dispose();
            }
            _modules = null;
        }
    }

    public void Init(HttpApplication context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        _modules = context.Application.GetServiceProvider()?.GetRequiredService<IEnumerable<IHttpModule>>();
        if (_modules is not null)
        {
            foreach (var module in _modules)
            {
                module.Init(context);
            }
        }
    }
}
