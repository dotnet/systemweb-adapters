// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text;

namespace System.Web;

public sealed class HttpCacheVaryByHeaders
{
    private bool _isModified;
    private bool _varyStar;
    private HttpDictionary? _headers;

    public HttpCacheVaryByHeaders()
    {
    }

    public void SetHeaders(string[]? headers)
    {
        int i, n;

        if (headers == null)
        {
            _isModified = false;
            _varyStar = false;
            _headers = null;
        }
        else
        {
            _isModified = true;
            if (headers[0].Equals("*", StringComparison.Ordinal))
            {
                Debug.Assert(headers.Length == 1, "headers.Length == 1");

                _varyStar = true;
                _headers = null;
            }
            else
            {
                _varyStar = false;
                _headers = new HttpDictionary();
                for (i = 0, n = headers.Length; i < n; i++)
                {
                    _headers.SetValue(headers[i], headers[i]);
                }
            }
        }
    }

    internal bool IsModified() => _isModified;

    internal string? ToHeaderString()
    {
        if (_varyStar)
        {
            return "*";
        }
        else if (_headers != null)
        {
            int i, n;
            var s = new StringBuilder();

            for (i = 0, n = _headers.Size; i < n; i++)
            {
                var item = _headers.GetValue(i);
                if (item != null)
                {
                    HttpCachePolicy.AppendValueToHeader(s, (string)item);
                }
            }

            if (s.Length > 0)
                return s.ToString();
        }

        return null;
    }

    public string[]? GetHeaders()
    {
        string[]? s = null;
        int i, j, c, n;

        if (_varyStar)
        {
            return new string[1] { "*" };
        }
        else if (_headers != null)
        {
            n = _headers.Size;
            c = 0;

            for (i = 0; i < n; i++)
            {
                var item = _headers.GetValue(i);
                if (item != null)
                {
                    c++;
                }
            }

            if (c > 0)
            {
                s = new string[c];
                j = 0;
                for (i = 0; i < n; i++)
                {
                    var item = _headers.GetValue(i);
                    if (item != null)
                    {
                        s[j] = (string)item;
                        j++;
                    }
                }

                Debug.Assert(j == c, "j == c");
            }
        }

        return s;
    }

    public void VaryByUnspecifiedParameters()
    {
        _isModified = true;
        _varyStar = true;
        _headers = null;
    }

    internal bool GetVaryByUnspecifiedParameters() => _varyStar;

    public bool AcceptTypes
    {
        get => this["Accept"];
        set
        {
            _isModified = true;
            this["Accept"] = value;
        }
    }

    public bool UserLanguage
    {
        get => this["Accept-Language"];
        set
        {
            _isModified = true;
            this["Accept-Language"] = value;
        }
    }

    public bool UserAgent
    {
        get => this["User-Agent"];
        set
        {
            _isModified = true;
            this["User-Agent"] = value;
        }
    }

    public bool UserCharSet
    {
        get => this["Accept-Charset"];
        set
        {
            _isModified = true;
            this["Accept-Charset"] = value;
        }
    }

    public bool this[string header]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(header);

            if (header.Equals("*", StringComparison.Ordinal))
            {
                return _varyStar;
            }
            else
            {
                return (_headers != null && _headers.GetValue(header) != null);
            }
        }

        set
        {
            ArgumentNullException.ThrowIfNull(header);

            /*
             * Since adding a Vary header is more restrictive, we don't
             * want components to be able to set a Vary header to false
             * if another component has set it to true.
             */
            if (value == false)
            {
                return;
            }

            _isModified = true;

            if (header.Equals("*", StringComparison.Ordinal))
            {
                VaryByUnspecifiedParameters();
            }
            else
            {
                // set value to header if true or null if false
                if (!_varyStar)
                {
                    if (_headers == null)
                    {
                        _headers = new HttpDictionary();
                    }

                    _headers.SetValue(header, header);
                }
            }
        }
    }
}
