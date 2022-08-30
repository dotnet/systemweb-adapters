// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.Diagnostics;

internal readonly struct DiagnosticResult
{
    public DiagnosticResult(DiagnosticStatus status, string name,object data)
    { 
        Status = status;
        Data = data;
        Name = name;
    }

    public string Name { get; }

    public DiagnosticStatus Status { get; }

    public object Data { get; }
}
