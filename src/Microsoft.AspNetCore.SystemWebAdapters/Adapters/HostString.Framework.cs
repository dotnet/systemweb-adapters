// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Represents the host portion of a URI can be used to construct URI's properly formatted and encoded for use in
/// HTTP headers.
/// </summary>
/// <remarks>
/// Copied from https://github.com/dotnet/aspnetcore/blob/0e255c5e3b86565a8d930f403de79bfc8b02cef5/src/Http/Http.Abstractions/src/HostString.cs
/// </remarks>
internal readonly struct HostString : IEquatable<HostString>
{
    private readonly string _value;

    /// <summary>
    /// Creates a new HostString without modification. The value should be Unicode rather than punycode, and may have a port.
    /// IPv4 and IPv6 addresses are also allowed, and also may have ports.
    /// </summary>
    /// <param name="value"></param>
    public HostString(string value)
    {
        _value = value;

        GetParts(_value, out var host, out var port);

        Host = host;

        if (!string.IsNullOrEmpty(port)
              && int.TryParse(port, NumberStyles.None, CultureInfo.InvariantCulture, out var p))
        {
            Port = p;
        }
        else
        {
            Port = null;
        }
    }

    /// <summary>
    /// Returns true if the host is set.
    /// </summary>
    public bool HasValue
    {
        get { return !string.IsNullOrEmpty(_value); }
    }

    /// <summary>
    /// Returns the value of the host part of the value. The port is removed if it was present.
    /// IPv6 addresses will have brackets added if they are missing.
    /// </summary>
    /// <returns>The host portion of the value.</returns>
    public string Host { get; }

    /// <summary>
    /// Returns the value of the port part of the host, or <value>null</value> if none is found.
    /// </summary>
    /// <returns>The port portion of the value.</returns>
    public int? Port { get; }

    /// <summary>
    /// Creates a new HostString from the given URI component.
    /// Any punycode will be converted to Unicode.
    /// </summary>
    /// <param name="uriComponent">The URI component string to create a <see cref="HostString"/> from.</param>
    /// <returns>The <see cref="HostString"/> that was created.</returns>
    public static HostString FromUriComponent(string uriComponent)
    {
        if (!string.IsNullOrEmpty(uriComponent))
        {
            int index;
            if (uriComponent.Contains("["))
            {
                // IPv6 in brackets [::1], maybe with port
            }
            else if ((index = uriComponent.IndexOf(':')) >= 0
                && index < uriComponent.Length - 1
                && uriComponent.IndexOf(':', index + 1) >= 0)
            {
                // IPv6 without brackets ::1 is the only type of host with 2 or more colons
            }
            else if (uriComponent.Contains("xn--"))
            {
                // Contains punycode
                if (index >= 0)
                {
                    // Has a port
                    string port = uriComponent.Substring(index);
                    var mapping = new IdnMapping();
                    uriComponent = mapping.GetUnicode(uriComponent, 0, index) + port;
                }
                else
                {
                    var mapping = new IdnMapping();
                    uriComponent = mapping.GetUnicode(uriComponent);
                }
            }
        }
        return new HostString(uriComponent);
    }

    /// <summary>
    /// Compares the equality of the Value property, ignoring case.
    /// </summary>
    /// <param name="other">The <see cref="HostString"/> to compare against.</param>
    /// <returns><see langword="true" /> if they have the same value.</returns>
    public bool Equals(HostString other)
    {
        if (!HasValue && !other.HasValue)
        {
            return true;
        }
        return string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Compares against the given object only if it is a HostString.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare against.</param>
    /// <returns><see langword="true" /> if they have the same value.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return !HasValue;
        }
        return obj is HostString && Equals((HostString)obj);
    }

    /// <summary>
    /// Gets a hash code for the value.
    /// </summary>
    /// <returns>The hash code as an <see cref="int"/>.</returns>
    public override int GetHashCode()
    {
        return (HasValue ? StringComparer.OrdinalIgnoreCase.GetHashCode(_value) : 0);
    }

    /// <summary>
    /// Compares the two instances for equality.
    /// </summary>
    /// <param name="left">The left parameter.</param>
    /// <param name="right">The right parameter.</param>
    /// <returns><see langword="true" /> if both <see cref="HostString"/>'s have the same value.</returns>
    public static bool operator ==(HostString left, HostString right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Compares the two instances for inequality.
    /// </summary>
    /// <param name="left">The left parameter.</param>
    /// <param name="right">The right parameter.</param>
    /// <returns><see langword="true" /> if both <see cref="HostString"/>'s values are not equal.</returns>
    public static bool operator !=(HostString left, HostString right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Parses the current value. IPv6 addresses will have brackets added if they are missing.
    /// </summary>
    /// <param name="value">The value to get the parts of.</param>
    /// <param name="host">The portion of the <paramref name="value"/> which represents the host.</param>
    /// <param name="port">The portion of the <paramref name="value"/> which represents the port.</param>
    private static void GetParts(string value, out string host, out string? port)
    {
        int index;
        port = null;
        host = null!;

        if (string.IsNullOrEmpty(value))
        {
            return;
        }
        else if ((index = value.IndexOf(']')) >= 0)
        {
            // IPv6 in brackets [::1], maybe with port
            host = value.Substring(0, index + 1);
            // Is there a colon and at least one character?
            if (index + 2 < value.Length && value[index + 1] == ':')
            {
                port = value.Substring(index + 2);
            }
        }
        else if ((index = value.IndexOf(':')) >= 0
            && index < value.Length - 1
            && value.IndexOf(':', index + 1) >= 0)
        {
            // IPv6 without brackets ::1 is the only type of host with 2 or more colons
            host = $"[{value}]";
            port = null;
        }
        else if (index >= 0)
        {
            // Has a port
            host = value.Substring(0, index);
            port = value.Substring(index + 1);
        }
        else
        {
            host = value;
            port = null;
        }
    }
}
