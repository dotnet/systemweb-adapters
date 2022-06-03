// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class SessionSerializerExtensions
{
    public static ISystemWebAdapterBuilder AddSessionSerializer(this ISystemWebAdapterBuilder builder, Action<SessionSerializerOptions>? configure = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var options = builder.Services.AddOptions<SessionSerializerOptions>();

        if (configure is not null)
        {
            options.Configure(configure);
        }

        builder.Services.TryAddSingleton<ISessionSerializer, BinarySessionSerializer>();

        return builder;
    }

    public static ISystemWebAdapterBuilder AddJsonSessionSerializer(this ISystemWebAdapterBuilder builder, Action<JsonSessionSerializerOptions>? configure = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.AddSessionSerializer();
        builder.Services.TryAddSingleton<ISessionKeySerializer, JsonSessionKeySerializer>();

        var options = builder.Services.AddOptions<JsonSessionSerializerOptions>();

        if (configure is not null)
        {
            options.Configure(configure);
        }

        return builder;
    }
}
