// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Diagnostics;

internal class VirtualDirectoryDiagnostic : IClientDiagnostic, IServerDiagnostic
{
    private const string HeaderName = "X-SystemWebAdapters-Diagnostic-VirtualDirectory";

    public string Name => "Virtual Directory Setup";

    void IClientDiagnostic.Prepare(HttpRequestMessage request)
    {
    }

    DiagnosticResult IClientDiagnostic.Process(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues(HeaderName, out var value) && value.FirstOrDefault() is { } serverAppPath)
        {
            var result = Result.Create(serverAppPath);

            return new DiagnosticResult(result.IsValid ? DiagnosticStatus.Healthy : DiagnosticStatus.Unhealthy, Name, result);
        }
        else
        {
            return new DiagnosticResult(DiagnosticStatus.Unhealthy, Name, Result.Unavailable);
        }
    }

    void IServerDiagnostic.Process(HttpContextBase context)
    {
        context.Response.Headers[HeaderName] = HttpRuntime.AppDomainAppVirtualPath;
    }

    private class Result
    {
        public static Result Unavailable = new(HttpRuntime.AppDomainAppVirtualPath, string.Empty);
        public static Result Same = new(HttpRuntime.AppDomainAppVirtualPath, HttpRuntime.AppDomainAppVirtualPath);

        public static Result Create(string server)
        {
            if (string.Equals(HttpRuntime.AppDomainAppVirtualPath, server, StringComparison.Ordinal))
            {
                return Same;
            }

            return new Result(HttpRuntime.AppDomainAppVirtualPath, server);
        }

        public Result(string client, string server)
        {
            Client = client;
            Server = server;
        }

        public bool IsValid => string.Equals(Client, Server, StringComparison.Ordinal);

        public string Client { get; }

        public string Server { get; }
    }
}
