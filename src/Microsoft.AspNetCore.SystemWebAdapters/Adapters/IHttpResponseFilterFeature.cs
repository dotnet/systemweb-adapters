// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal interface IHttpResponseFilterFeature
{
    [AllowNull]
    Stream Filter { get; set; }
}

#endif
