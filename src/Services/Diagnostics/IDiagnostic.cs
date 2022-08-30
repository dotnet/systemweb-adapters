// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.Diagnostics;

internal interface IDiagnostic
{
    IEnumerable<DiagnosticResult> Process();
}
