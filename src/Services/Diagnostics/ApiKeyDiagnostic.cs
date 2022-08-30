// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Diagnostics;

internal class ApiKeyDiagnostic : IClientDiagnostic, IServerDiagnostic
{
    public string Name => "API Key";

    void IClientDiagnostic.Prepare(HttpRequestMessage request)
    {
    }

    DiagnosticResult IClientDiagnostic.Process(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new DiagnosticResult(DiagnosticStatus.Unhealthy, Name, Result.Unauthorized);
        }
        else if (response.StatusCode == HttpStatusCode.OK)
        {
            return new DiagnosticResult(DiagnosticStatus.Healthy, Name, Result.Authorized);
        }
        else
        {
            return new DiagnosticResult(DiagnosticStatus.Degraded, Name, new Result(response.StatusCode));
        }

    }

    void IServerDiagnostic.Process(HttpContextBase context)
    {
    }

    private class Result
    {
        public static Result Authorized = new(HttpStatusCode.OK);
        public static Result Unauthorized = new(HttpStatusCode.Unauthorized);

        public Result(HttpStatusCode status)
        {
            Status = status;
        }

        public HttpStatusCode Status { get; }
    }
}
