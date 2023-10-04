// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web.Configuration;

public class HttpCapabilitiesBase
{
    private readonly HttpContextCore _context;

    private FeatureReference<IHttpBrowserCapabilityFeature> _capability;

    private protected HttpCapabilitiesBase(HttpContextCore context)
    {
        _context = context;
        _capability = FeatureReference<IHttpBrowserCapabilityFeature>.Default;
    }

    private IHttpBrowserCapabilityFeature Capability
    {
        get
        {
            if (_capability.Fetch(_context.Features) is { } existing)
            {
                return existing;
            }

            return _capability.Update(_context.Features, _context.RequestServices.GetRequiredService<IBrowserCapabilitiesFactory>().Create(_context.Request));
        }
    }

    public string? Browser => Capability["browser"];

    public string? Version => Capability["version"];

    public int MajorVersion => GetInt("majorversion");

    public double MinorVersion => GetDouble("minorversion");

    public string? Platform => Capability["platform"];

    public bool Crawler => GetBoolean("crawler");

    public bool Cookies => GetBoolean("cookies");

    public string? Type => Capability["type"];

    public string? PreferredRequestEncoding => Capability["preferredRequestEncoding"];

    public string? this[string key] => Capability[key];

    public Version EcmaScriptVersion => GetVersion("ecmascriptversion");

    public Version MSDomVersion => GetVersion("msdomversion");

    public Version W3CDomVersion => GetVersion("w3cdomversion");

    public bool SupportsCallback => GetBoolean("supportsCallback");

    public bool IsMobileDevice => GetBoolean("isMobileDevice");

    private int GetInt(string key) => int.TryParse(Capability[key], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : throw new HttpUnhandledException($"Invalid string from browser capabilities '{key}'");

    private bool GetBoolean(string key) => bool.TryParse(Capability[key], out var result) && result;

    private double GetDouble(string key)
    {
        const NumberStyles Style = NumberStyles.Float | NumberStyles.AllowDecimalPoint;

        var value = Capability[key];

        if (value is not null)
        {
            if (double.TryParse(value, Style, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            // Handle if there's more than one decimal i.e. .4.1 -> .4
            var firstDecimal = value.IndexOf('.', StringComparison.Ordinal);

            if (firstDecimal != -1)
            {
                var nextDecimal = value.IndexOf('.', firstDecimal + 1);

                if (nextDecimal != -1)
                {
                    if (double.TryParse(value.AsSpan()[..nextDecimal], Style, CultureInfo.InvariantCulture, out var result2))
                    {
                        return result2;
                    }
                }
            }
        }

        throw new HttpUnhandledException($"Invalid string from browser capabilities '{key}'");
    }

    private Version GetVersion(string key)
    {
        var result = Capability[key];
        if (result is not null)
        {
            return new Version(result);
        }
        else
        {
            throw new ArgumentNullException(nameof(key));
        }
    }
}
