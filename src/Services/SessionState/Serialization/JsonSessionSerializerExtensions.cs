// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.DependencyInjection.Extensions;

#if NETFRAMEWORK
using Microsoft.Extensions.DependencyInjection;

namespace System.Web;
#else
using System;


namespace Microsoft.Extensions.DependencyInjection;
#endif

public static class JsonSessionSerializerExtensions
{
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
