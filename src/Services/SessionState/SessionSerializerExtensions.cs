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

public static class SessionSerializerExtensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1510:Use ArgumentNullException throw helper", Justification = "Source shared with .NET Framework that does not have the method")]
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
}
