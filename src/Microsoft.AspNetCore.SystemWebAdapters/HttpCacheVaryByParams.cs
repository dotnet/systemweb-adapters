// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace System.Web;

public sealed class HttpCacheVaryByParams
{
    private HttpDictionary? _parameters;
    private int _ignoreParams;
    private bool _isModified;

    public HttpCacheVaryByParams()
    {
        Reset();
    }

    internal void Reset()
    {
        _isModified = false;
        IsVaryByStar = false;
        _parameters = null;
        _ignoreParams = -1;
    }

    public void SetParams(string[]? parameters)
    {
        int i, n;

        Reset();
        if (parameters != null)
        {
            _isModified = true;
            if (parameters[0].Length == 0)
            {
                Debug.Assert(parameters.Length == 1, "parameters.Length == 1");

                IgnoreParams = true;
            }
            else if (parameters[0].Equals("*", StringComparison.Ordinal))
            {
                Debug.Assert(parameters.Length == 1, "parameters.Length == 1");

                IsVaryByStar = true;
            }
            else
            {
                _parameters = new HttpDictionary();
                for (i = 0, n = parameters.Length; i < n; i++)
                {
                    _parameters.SetValue(parameters[i], parameters[i]);
                }
            }
        }
    }

    internal bool IsModified() => _isModified;

    internal bool AcceptsParams() => _ignoreParams == 1 || IsVaryByStar || _parameters != null;

    public string[]? GetParams()
    {
        string[]? s = null;
        int i, j, c, n;

        if (_ignoreParams == 1)
        {
            s = [string.Empty];
        }
        else if (IsVaryByStar)
        {
            s = ["*"];
        }
        else if (_parameters != null)
        {
            n = _parameters.Size;
            c = 0;
            for (i = 0; i < n; i++)
            {
                var item = _parameters.GetValue(i);
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
                    var item = _parameters.GetValue(i);
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

    public bool this[string header]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(header);

            if (header.Length == 0)
            {
                return _ignoreParams == 1;
            }
            else
            {
                return IsVaryByStar ||
                       (_parameters != null && _parameters.GetValue(header) != null);
            }
        }

        set
        {
            ArgumentNullException.ThrowIfNull(header);

            if (header.Length == 0)
            {
                IgnoreParams = value;
            }

            /*
             * Since adding a Vary parameter is more restrictive, we don't
             * want components to be able to set a Vary parameter to false
             * if another component has set it to true.
             */
            else if (value)
            {
                _isModified = true;
                _ignoreParams = 0;

                if (header.Equals("*", StringComparison.Ordinal))
                {
                    IsVaryByStar = true;
                    _parameters = null;
                }
                else
                {
                    // set value to header if true or null if false
                    if (!IsVaryByStar)
                    {
                        _parameters ??= new HttpDictionary();

                        _parameters.SetValue(header, header);
                    }
                }
            }
        }
    }


    public bool IgnoreParams
    {
        get => _ignoreParams == 1;

        set
        {
            // Don't ignore if params have been added
            if (IsVaryByStar || _parameters != null)
            {
                return;
            }

            if (_ignoreParams == -1 || _ignoreParams == 1)
            {
                _ignoreParams = value ? 1 : 0;
                _isModified = true;
            }
        }
    }

    internal bool IsVaryByStar { get; private set; }
}
