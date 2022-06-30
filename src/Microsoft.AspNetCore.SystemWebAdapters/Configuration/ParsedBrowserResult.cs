// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace System.Web.Configuration;

internal class ParsedBrowserResult
{
    private readonly Dictionary<string, string?> _data = new(StringComparer.OrdinalIgnoreCase);

    [Conditional("NotNeededYet")]
    [Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Retained in case we need the data later")]
    public void AddBrowser(string browser)
    {
    }

    public string? this[string key]
    {
        get => _data.TryGetValue(key, out var value) ? value : null;
        set => _data[key] = value;
    }

    public int GetInt(string key) => int.TryParse(this[key], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : throw new HttpUnhandledException($"Invalid string from browser capabilities '{key}'");

    public bool GetBoolean(string key) => bool.TryParse(this[key], out var result) && result;

    public double GetDouble(string key)
    {
        const NumberStyles Style = NumberStyles.Float | NumberStyles.AllowDecimalPoint;

        var value = this[key];

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
}
