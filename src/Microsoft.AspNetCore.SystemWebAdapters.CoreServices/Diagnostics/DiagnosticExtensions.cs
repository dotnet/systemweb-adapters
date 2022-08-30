// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection;

internal static class DiagnosticExtensions
{
    public static ISystemWebAdapterBuilder AddDiagnostics(this ISystemWebAdapterBuilder builder)
    {
        builder.Services.AddHealthChecks();

        return builder;
    }

    internal static ISystemWebAdapterRemoteClientAppBuilder AddRemoteAppDiagnostics(this ISystemWebAdapterRemoteClientAppBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck<RemoteDiagnosticHealthCheck>("SystemWebAdapters-RemoteApp");

        builder.Services.AddTransient<IClientDiagnostic, VirtualDirectoryDiagnostic>();
        builder.Services.AddTransient<IClientDiagnostic, ApiKeyDiagnostic>();
        builder.Services.AddTransient<IClientDiagnostic, VersionDiagnostic>();

        return builder;
    }
}
