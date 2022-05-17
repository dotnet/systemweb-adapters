// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class RemoteAuthenticationExtensions
{
    public static ISystemWebAdapterBuilder AddRemoteAuthentication(this ISystemWebAdapterBuilder builder, Action<RemoteAuthenticationOptions> configureRemoteAuthentication)
    {
        var options = new RemoteAuthenticationOptions();
        configureRemoteAuthentication(options);

        builder.Modules.Add(new RemoteAuthenticationModule(options));
        return builder;
    }
}
