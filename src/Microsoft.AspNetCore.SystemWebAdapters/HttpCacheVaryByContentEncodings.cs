// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

public sealed class HttpCacheVaryByContentEncodings
{
    private string[]? _contentEncodings;
    private bool _isModified;

    public HttpCacheVaryByContentEncodings()
    {
        Reset();
    }

    internal void Reset()
    {
        _isModified = false;
        _contentEncodings = null;
    }

    /// <summary>
    /// Set the Content Encodings in Cache Vary
    /// </summary>
    /// <param name="contentEncodings"></param>
    public void SetContentEncodings(string[]? contentEncodings)
    {
        Reset();

        if (contentEncodings != null)
        {
            _isModified = true;
            _contentEncodings = new string[contentEncodings.Length];
            for (var i = 0; i < contentEncodings.Length; i++)
            {
                _contentEncodings[i] = contentEncodings[i];
            }
        }
    }

    // the response is not cacheable if we're varying by content encoding
    // and the content-encoding header is not one of the encodings that we're
    // varying by
    internal bool IsCacheableEncoding(string coding)
    {
        // return true if we are not varying by content encoding.
        if (_contentEncodings == null)
        {
            return true;
        }

        // return true if there is no Content-Encoding header
        if (coding == null)
        {
            return true;
        }

        // return true if the Content-Encoding header is listed
        for (var i = 0; i < _contentEncodings.Length; i++)
        {
            if (_contentEncodings[i] == coding)
            {
                return true;
            }
        }

        // return false if the Content-Encoding header is not listed
        return false;
    }

    internal bool IsModified() => _isModified;

    public string[]? GetContentEncodings()
    {
        if (_contentEncodings != null)
        {
            var contentEncodings = new string[_contentEncodings.Length];
            _contentEncodings.CopyTo(contentEncodings, 0);
            return contentEncodings;
        }
        return null;
    }

    public bool this[string contentEncoding]
    {
        get
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(contentEncoding);

            if (_contentEncodings == null)
            {
                return false;
            }
            for (var i = 0; i < _contentEncodings.Length; i++)
            {
                if (_contentEncodings[i] == contentEncoding)
                {
                    return true;
                }
            }
            return false;
        }

        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(contentEncoding);

            // if someone enabled it, don't allow someone else to disable it.
            if (!value)
            {
                return;
            }

            _isModified = true;
            if (_contentEncodings != null)
            {
                var contentEncodings = new string[_contentEncodings.Length + 1];
                for (var i = 0; i < _contentEncodings.Length; i++)
                {
                    contentEncodings[i] = _contentEncodings[i];
                }
                contentEncodings[contentEncodings.Length - 1] = contentEncoding;
                _contentEncodings = contentEncodings;
                return;
            }
            _contentEncodings = new string[1];
            _contentEncodings[0] = contentEncoding;
        }
    }
}
