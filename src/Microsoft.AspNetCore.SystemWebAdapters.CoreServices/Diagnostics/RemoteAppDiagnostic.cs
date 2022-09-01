// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.Diagnostics;

internal class RemoteAppDiagnostic : IDiagnostic
{
    private readonly IHttpClientFactory _factory;
    private readonly IOptions<DiagnosticOptions> _options;
    private readonly IEnumerable<IClientDiagnostic> _diagnostics;

    public RemoteAppDiagnostic(IEnumerable<IClientDiagnostic> diagnostics, IOptions<DiagnosticOptions> options, IHttpClientFactory factory)
    {
        _factory = factory;
        _options = options;
        _diagnostics = diagnostics;
    }

    public async IAsyncEnumerable<DiagnosticResult> ProcessAsync([EnumeratorCancellation] CancellationToken token)
    {
        using var client = _factory.CreateClient(RemoteConstants.HttpClientName);
        using var message = new HttpRequestMessage(HttpMethod.Get, _options.Value.Path);

        foreach (var diagnostic in _diagnostics)
        {
            diagnostic.Prepare(message);
        }

        using var response = await client.SendAsync(message, token);

        foreach (var diagnostic in _diagnostics)
        {
            yield return diagnostic.Process(response);
        }
    }
}
