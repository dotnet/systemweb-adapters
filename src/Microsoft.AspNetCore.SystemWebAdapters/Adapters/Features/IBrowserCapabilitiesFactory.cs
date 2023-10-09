// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP
namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

internal interface IBrowserCapabilitiesFactory
{
    IHttpBrowserCapabilityFeature Create(HttpRequestCore request);
}
#endif
