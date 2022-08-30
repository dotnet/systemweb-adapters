// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.Diagnostics;

internal class DiagnosticsModule : RemoteModule, IHttpHandler
{
    private readonly IEnumerable<IServerDiagnostic> _diagnostics;

    public DiagnosticsModule(IOptions<RemoteAppServerOptions> remoteOptions, IOptions<DiagnosticOptions> options, IEnumerable<IServerDiagnostic> diagnostics)
        : base(remoteOptions)
    {
        _diagnostics = diagnostics;

        Path = options.Value.Path;

        MapGet(_ => this);
    }

    protected override string Path { get; }

    bool IHttpHandler.IsReusable => true;

    public void ProcessRequest(HttpContextBase context)
    {
        foreach (var diagnostic in _diagnostics)
        {
            diagnostic.Process(context);
        }
    }

    void IHttpHandler.ProcessRequest(HttpContext context) => ProcessRequest(new HttpContextWrapper(context));
}
