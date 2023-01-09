// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Wrapped;

namespace Microsoft.Extensions.DependencyInjection;

public static class ISystemWebAdapterBuilderSessionExtensions
{
    public static ISystemWebAdapterBuilder WrapAspNetCoreSession(this ISystemWebAdapterBuilder builder, Action<SessionOptions>? options = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

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
