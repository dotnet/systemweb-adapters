// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters.Diagnostics;

internal class SessionDiagnostic : IClientDiagnostic, IServerDiagnostic, IDiagnostic
{
    private const string HeaderName = "X-SystemWebAdapters-Diagnostic-Session";
    private const string UnknownHeaderName = "X-SystemWebAdapters-Diagnostic-Session-Unknown";

    private readonly IUnknownKeyTracker _unknown;

    public string Name => "RemoteSession";

    public SessionDiagnostic(IUnknownKeyTracker unknownKeys)
    {
        _unknown = unknownKeys;
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    async IAsyncEnumerable<DiagnosticResult> IDiagnostic.ProcessAsync([EnumeratorCancellation] CancellationToken token)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        const string Name = "Session";

        if (TryGetUnknownSession(out var unknown))
        {
            yield return new DiagnosticResult(DiagnosticStatus.Degraded, Name, unknown);
        }
        else
        {
            yield return new DiagnosticResult(DiagnosticStatus.Healthy, Name, Result.Valid);
        }
    }

    private bool TryGetUnknownSession(out UnknownSession unknown)
    {
        var keys = _unknown.UnknownKeys;
        var types = _unknown.UnknownTypes;

        if (keys.Count > 0 || types.Count > 0)
        {
            var unknownTypes = types.Select(u => new UnknownType(u.Key, u.Select(static t => t.Name)));

            unknown = new UnknownSession { UnknownKeys = keys, UnknownTypes = unknownTypes };
            return true;
        }

        unknown = default!;
        return false;
    }

    void IClientDiagnostic.Prepare(HttpRequestMessage request)
    {
    }

    DiagnosticResult IClientDiagnostic.Process(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues(HeaderName, out var result) && bool.TryParse(result.FirstOrDefault(), out var isAvailable) && isAvailable)
        {
            if (response.Headers.TryGetValues(UnknownHeaderName, out var allUnknown) && allUnknown.FirstOrDefault() is { } unknown)
            {
                var holder = JsonSerializer.Deserialize<UnknownSession>(unknown);

                if (holder is not null)
                {
                    return new DiagnosticResult(DiagnosticStatus.Degraded, Name, new Result("UnknownKeys", holder));
                }
            }

            return new DiagnosticResult(DiagnosticStatus.Healthy, Name, Result.Valid);
        }
        else
        {
            return new DiagnosticResult(DiagnosticStatus.Unhealthy, Name, Result.Unavailable);
        }
    }

    void IServerDiagnostic.Process(HttpContextBase context)
    {
        context.Response.Headers[HeaderName] = "true";

        if (TryGetUnknownSession(out var unknown))
        {
            var value = JsonSerializer.Serialize(unknown);

            context.Response.Headers[UnknownHeaderName] = value;
        }
    }

    private class UnknownSession
    {
        public IEnumerable<string> UnknownKeys { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<UnknownType> UnknownTypes { get; set; } = Enumerable.Empty<UnknownType>();
    }

    private class UnknownType
    {
        public UnknownType(string key, IEnumerable<string> types)
        {
            Key = key;
            Types = types;
        }

        public string Key { get; }

        public IEnumerable<string> Types { get; }
    }

    private class Result
    {
        public static Result Valid = new("NoIssue", new());
        public static Result Unavailable = new("Unavailable", new());

        public Result(string status, UnknownSession unknown)
        {
            Status = status;
            Details = unknown;
        }

        public string Status { get; }

        public UnknownSession Details { get; }
    }
}
