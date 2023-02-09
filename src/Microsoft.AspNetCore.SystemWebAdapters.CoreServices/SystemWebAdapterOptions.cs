// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class SystemWebAdapterOptions
{
    public ICollection<StringSegment> Extensions { get; } = new HashSet<StringSegment>(StringSegmentComparer.OrdinalIgnoreCase);
}
