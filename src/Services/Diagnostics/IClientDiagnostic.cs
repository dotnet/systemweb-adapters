// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.Diagnostics;

internal interface IClientDiagnostic
{
    void Prepare(HttpRequestMessage request);

    DiagnosticResult Process(HttpResponseMessage response);
}
