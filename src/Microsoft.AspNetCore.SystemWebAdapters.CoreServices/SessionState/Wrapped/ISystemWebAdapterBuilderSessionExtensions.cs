// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Wrapped;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ISystemWebAdapterBuilderSessionExtensions
{
    [Obsolete("Prefer AddWrappedAspNetCoreSession instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ISystemWebAdapterBuilder WrapAspNetCoreSession(this ISystemWebAdapterBuilder builder, Action<SessionOptions>? options = null)
        => builder.AddWrappedAspNetCoreSession(options);

    public static ISystemWebAdapterBuilder AddWrappedAspNetCoreSession(this ISystemWebAdapterBuilder builder, Action<SessionOptions>? options = null)
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

        builder.Services.TryAddSingleton<ICompositeSessionKeySerializer, CompositeSessionKeySerializer>();
        builder.Services.AddSingleton<ISessionManager, AspNetCoreSessionManager>();

        return builder;
    }
}
