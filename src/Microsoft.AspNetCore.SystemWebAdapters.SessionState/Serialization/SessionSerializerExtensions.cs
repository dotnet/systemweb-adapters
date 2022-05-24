// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class SessionSerializerExtensions
{
    public static ISystemWebAdapterBuilder AddJsonSessionSerializer(this ISystemWebAdapterBuilder builder, Action<SessionSerializerOptions>? configure = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.AddSingleton<ISessionSerializer, JsonSessionSerializer>();

        var options = builder.Services.AddOptions<SessionSerializerOptions>();

        if (configure is not null)
        {
            options.Configure(configure);
        }

        return builder;
    }
}
