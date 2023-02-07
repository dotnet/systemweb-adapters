// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters;

#if NET6_0_OR_GREATER
public interface IHttpBrowserCapabilityFeature
{
    string? this[string key] { get; }
}
#endif
