// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Wrapped;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class WrappedAspNetCoreSessionExtensions
{
    public static ISystemWebAdapterBuilder WrapAspNetCoreSession(this ISystemWebAdapterBuilder builder, Action<Builder.SessionOptions>? options = null)
    {
        if (options is null)
        {
            builder.Services.AddSession();
        }
        else
        {
            builder.Services.AddSession(options);
        }

        builder.Services.AddSingleton<ISessionManager, AspNetCoreSessionManager>();

        return builder;
    }
}
