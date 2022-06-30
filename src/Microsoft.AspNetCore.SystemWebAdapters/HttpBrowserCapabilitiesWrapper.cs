// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

public class HttpBrowserCapabilitiesWrapper : HttpBrowserCapabilitiesBase
{
    private readonly HttpBrowserCapabilities _capabilities;

    public HttpBrowserCapabilitiesWrapper(HttpBrowserCapabilities capabilities)
    {
        _capabilities = capabilities;
    }

    public override string? Browser => _capabilities.Browser;

    public override int MajorVersion => _capabilities.MajorVersion;

    public override double MinorVersion => _capabilities.MinorVersion;

    public override string? Platform => _capabilities.Platform;

    public override string? Version => _capabilities.Version;

    public override bool Crawler => _capabilities.Crawler;
}
