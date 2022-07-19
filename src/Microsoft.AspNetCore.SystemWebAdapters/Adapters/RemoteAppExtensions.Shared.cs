// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;

#if NETFRAMEWORK
using Microsoft.Extensions.DependencyInjection;

namespace System.Web;
#else
using System;

namespace Microsoft.Extensions.DependencyInjection;
#endif

public static class RemoteAppExtensions
{
    /// <summary>
    /// Add configuration for connecting to a remote app for System.Web extensions that require a remote
    /// ASP.NET app such as remote app authentication or remote app session sharing.
    /// </summary>
    public static ISystemWebAdapterBuilder AddRemoteApp(this ISystemWebAdapterBuilder builder, Action<RemoteAppOptions> configure)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var options = builder.Services.AddOptions<RemoteAppOptions>()
    #if NET6_0_OR_GREATER
            .ValidateDataAnnotations()
    #endif
            .Configure(configure);

        return builder;
    }
}
