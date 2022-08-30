// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web;

internal static class DiagnosticExtensions
{
    public static ISystemWebAdapterBuilder AddDiagnostics(this ISystemWebAdapterBuilder builder)
    {
        builder.Services.AddScoped<IHttpModule, DiagnosticsModule>();

        return builder;
    }

    public static ISystemWebAdapterBuilder AddRemoteDiagnostics(this ISystemWebAdapterBuilder builder)
    {
        builder.Services.AddTransient<IServerDiagnostic, VirtualDirectoryDiagnostic>();
        builder.Services.AddTransient<IServerDiagnostic, ApiKeyDiagnostic>();
        builder.Services.AddTransient<IServerDiagnostic, VersionDiagnostic>();

        return builder;
    }
}
