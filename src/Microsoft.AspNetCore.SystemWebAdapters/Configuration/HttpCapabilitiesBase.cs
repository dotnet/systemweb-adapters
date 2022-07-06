// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Configuration;

public class HttpCapabilitiesBase
{
    private readonly ParsedBrowserResult _data;

    private protected HttpCapabilitiesBase(BrowserCapabilitiesFactory factory, string userAgent)
    {
        _data = factory.Process(userAgent);
    }

    public string? Browser => _data["browser"];

    public string? Version => _data["version"];

    public int MajorVersion => _data.GetInt("majorversion");

    public double MinorVersion => _data.GetDouble("minorversion");

    public string? Platform => _data["platform"];

    public bool Crawler => _data.GetBoolean("crawler");
}
