// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class SystemWebAdapterModule : IHttpModule
{
    private ISystemWebAdapterBuilder? _builder;

    public void Dispose()
    {
        if (_builder is { } builder)
        {
            foreach (var module in builder.Modules)
            {
                module.Dispose();
            }

            _builder = null;
        }
    }

    public void Init(HttpApplication context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        _builder = context.Application.GetSystemWebBuilder();

        if (_builder is { } builder)
        {
            foreach (var module in builder.Modules)
            {
                module.Init(context);
            }
        }
    }
}
