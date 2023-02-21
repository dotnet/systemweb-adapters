// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public sealed record class MappedHttpHandlerOptions
{
    public bool AllowPathInfo { get; init; }

    public IEnumerable<string>? Verbs { get; init; }
}

