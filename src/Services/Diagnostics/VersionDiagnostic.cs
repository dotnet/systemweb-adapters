// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Diagnostics;

internal class VersionDiagnostic : IClientDiagnostic, IServerDiagnostic
{
    private const string ResultHeaderName = "X-SystemWebAdapters-Diagnostic-Version";
    private readonly Result _sameResult;
    private readonly Result _none = new(string.Empty, string.Empty);

    public VersionDiagnostic()
    {
        var attribute = typeof(VersionDiagnostic).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>() ?? throw new InvalidOperationException();
        Version = attribute.InformationalVersion;

        _sameResult = new Result(Version, Version);
    }

    public string Version { get; }

    public string Name => "Version";

    void IClientDiagnostic.Prepare(HttpRequestMessage request)
    {
    }

    DiagnosticResult IClientDiagnostic.Process(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues(ResultHeaderName, out var result) && result.FirstOrDefault() is string serverVersion)
        {
            if (string.Equals(serverVersion, Version, StringComparison.Ordinal))
            {
                return new DiagnosticResult(DiagnosticStatus.Healthy, Name, _sameResult);
            }
            else
            {
                return new DiagnosticResult(DiagnosticStatus.Unhealthy, Name, new Result(Version, serverVersion));
            }
        }
        else
        {
            return new DiagnosticResult(DiagnosticStatus.Degraded, Name, _none);
        }
    }

    void IServerDiagnostic.Process(HttpContextBase context)
    {
        context.Response.Headers[ResultHeaderName] = Version;
    }

    private class Result
    {
        public Result(string clientVersion, string serverVersion)
        {
            ClientVersion = clientVersion;
            ServerVersion = serverVersion;
        }

        public string ClientVersion { get; }

        public string ServerVersion { get; }
    }
}
