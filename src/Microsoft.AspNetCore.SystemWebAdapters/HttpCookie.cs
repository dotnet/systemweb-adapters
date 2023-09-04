// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using Microsoft.AspNetCore.SystemWebAdapters.Internal;
using Microsoft.Net.Http.Headers;

namespace System.Web;

public sealed class HttpCookie
{
    private string? _stringValue;
    private HttpValueCollection? _multiValue;

    public HttpCookie(string name)
    {
        Name = name;
    }

    public HttpCookie(string name, string? value)
    {
        Name = name;
        _stringValue = value;
    }

    /// <summary>
    /// Gets or sets the name of a cookie.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a subkey. Same as accessing the item from <see cref="Values"./>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string? this[string key]
    {
        get => Values[key];
        set => Values[key] = value;
    }

    /// <summary>
    /// Gets or sets an individual cookie value.
    /// </summary>
    public string? Value
    {
        get
        {
            if (_multiValue != null)
                return _multiValue.ToString(false);
            else
                return _stringValue;
        }

        set
        {
            if (_multiValue != null)
            {
                // reset multivalue collection to contain
                // single keyless value
                _multiValue.Clear();
                _multiValue.Add(null, value);
            }
            else
            {
                // remember as string
                _stringValue = value;
            }
        }
    }

    /// <summary>
    /// Gets a collection of key/value pairs that are contained within a single cookie object.
    /// </summary>
    public NameValueCollection Values
    {
        get
        {
            if (_multiValue == null)
            {
                // create collection on demand
                _multiValue = new HttpValueCollection();

                // convert existing string value into multivalue
                if (_stringValue != null)
                {
                    if (_stringValue.Contains('&', StringComparison.InvariantCulture) || _stringValue.Contains('=', StringComparison.InvariantCulture))
                        _multiValue.FillFromString(_stringValue);
                    else
                        _multiValue.Add(null, _stringValue);

                    _stringValue = null;
                }
            }

            return _multiValue;
        }
    }

    internal void CopyTo(HttpValueCollection other)
    {
        if (_stringValue != null)
        {
            other.Add(null, _stringValue);
        }
        else if (_multiValue != null)
        {
            for (var i = 0; i < _multiValue.Count; i++)
            {
                other.Add(_multiValue.GetKey(i), _multiValue[i]);
            }
        }
    }

    /// <summary>
    /// Gets or sets the expiration date and time for the cookie.
    /// </summary>
    public DateTime Expires { get; set; }

    /// <summary>
    /// Gets or sets a value that specifies whether a cookie is accessible by client-side script.
    /// </summary>
    public bool HttpOnly { get; set; }

    /// <summary>
    /// Gets or sets the virtual path to transmit with the current cookie.
    /// </summary>
    public string Path { get; set; } = "/";

    /// <summary>
    /// Gets or sets a value indicating whether to transmit the cookie using Secure Sockets Layer (SSL)--that is, over HTTPS only.
    /// </summary>
    public bool Secure { get; set; }

    /// <summary>
    /// Gets or sets the value for the SameSite attribute of the cookie.
    /// </summary>
    public SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;

    /// <summary>
    /// Gets a value indicating whether the cookie has sub-keys.
    /// </summary>
    public bool HasKeys => Values?.HasKeys() ?? false;

    /// <summary>
    /// Gets a value indicating whether this cookie is allowed to participate in output caching.
    /// </summary>
    public bool Shareable { get; set; }

    /// <summary>
    /// Gets or sets the domain to associate the cookie with.
    /// </summary>
    public string? Domain { get; set; }

    internal SetCookieHeaderValue ToSetCookieHeaderValue() => new(Name, Value)
    {
        Domain = Domain,
        Expires = (Expires == DateTime.MinValue) ? null : new DateTimeOffset(Expires),
        HttpOnly = HttpOnly,
        Path = Path,
        SameSite = (Microsoft.Net.Http.Headers.SameSiteMode)SameSite,
        Secure = Secure,
    };
}
