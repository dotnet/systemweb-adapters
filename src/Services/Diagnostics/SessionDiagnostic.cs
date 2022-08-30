// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.SystemWebAdapters.Diagnostics;

internal class SessionDiagnostic : IClientDiagnostic, IServerDiagnostic
{
    private const string HeaderName = "X-SystemWebAdapters-Diagnostic-Session";
    private const string UnknownHeaderName = "X-SystemWebAdapters-Diagnostic-Session-Unknown";

    private readonly IUnknownKeyTracker _unknown;

    public string Name => "RemoteSession";

    public SessionDiagnostic(IUnknownKeyTracker unknownKeys)
    {
        _unknown = unknownKeys;
    }

    void IClientDiagnostic.Prepare(HttpRequestMessage request)
    {
    }

    DiagnosticResult IClientDiagnostic.Process(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues(HeaderName, out var result) && bool.TryParse(result.FirstOrDefault(), out var isAvailable) && isAvailable)
        {
            if (response.Headers.TryGetValues(UnknownHeaderName, out var unknown))
            {
                return new DiagnosticResult(DiagnosticStatus.Degraded, Name, new Result("UnknownKeys", unknown));
            }
            else
            {
                return new DiagnosticResult(DiagnosticStatus.Healthy, Name, Result.Valid);
            }
        }
        else
        {
            return new DiagnosticResult(DiagnosticStatus.Unhealthy, Name, Result.Unavailable);
        }
    }

    void IServerDiagnostic.Process(HttpContextBase context)
    {
        context.Response.Headers[HeaderName] = "true";

        foreach (var unknown in _unknown.UnknownKeys)
        {
            context.Response.Headers[UnknownHeaderName] = unknown;
        }
    }

    private class Result
    {
        public static Result Valid = new("NoIssue", Enumerable.Empty<string>());
        public static Result Unavailable = new("Unavailable", Enumerable.Empty<string>());

        public Result(string status, IEnumerable<string> unknownKeys)
        {
            Status = status;
            UnknownKeys = unknownKeys;
        }

        public string Status { get; }

        public IEnumerable<string> UnknownKeys { get; }
    }
}
