// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace System.Web;

public delegate void HttpCacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus);

internal sealed record HttpResponseHeader(string HeaderName, string Value);

[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = Constants.ApiFromAspNet)]
public sealed class HttpCachePolicy
{
    private static readonly TimeSpan s_oneYear = new TimeSpan(TimeSpan.TicksPerDay * 365);
    private static readonly HttpResponseHeader s_headerPragmaNoCache = new(HeaderNames.Pragma, "no-cache");
    private static readonly HttpResponseHeader s_headerExpiresMinus1 = new(HeaderNames.Expires, "-1");

    private bool _isModified;
    private bool _noServerCaching;
    private string? _cacheExtension;
    private bool _noTransforms;
    private readonly bool _ignoreRangeRequests;
    private string? _varyByCustom;

    private HttpCacheability _cacheability;
    private bool _noStore;
    private HttpDictionary? _privateFields;
    private HttpDictionary? _noCacheFields;

    private DateTime _utcExpires;
    private bool _isExpiresSet;
    private TimeSpan _maxAge;
    private bool _isMaxAgeSet;
    private TimeSpan _proxyMaxAge;
    private bool _isProxyMaxAgeSet;
    private int _slidingExpiration;
    private TimeSpan _slidingDelta;
    private DateTime _utcTimestampRequest;
    private int _validUntilExpires;
    private int _allowInHistory;

    private HttpCacheRevalidation _revalidation;
    private DateTime _utcLastModified;
    private bool _isLastModifiedSet;
    private string? _etag;

    private bool _generateLastModifiedFromFiles;
    private bool _generateEtagFromFiles;
    private int _omitVaryStar;

    private List<ValidationCallbackInfo>? _validationCallbackInfo;

    private bool _useCachedHeaders;
    private HttpResponseHeader? _headerCacheControl;
    private HttpResponseHeader? _headerPragma;
    private HttpResponseHeader? _headerExpires;
    private HttpResponseHeader? _headerLastModified;
    private HttpResponseHeader? _headerEtag;
    private HttpResponseHeader? _headerVaryBy;

    private readonly bool _noMaxAgeInCacheControl;

    internal HttpCachePolicy()
    {
        VaryByContentEncodings = new HttpCacheVaryByContentEncodings();
        VaryByHeaders = new HttpCacheVaryByHeaders();
        VaryByParams = new HttpCacheVaryByParams();
    
        _isModified = false;
        _noServerCaching = false;
        _cacheExtension = null;
        _noTransforms = false;
        _ignoreRangeRequests = false;
        _varyByCustom = null;
        _cacheability = (HttpCacheability)(int)HttpCacheabilityLimits.None;
        _noStore = false;
        _privateFields = null;
        _noCacheFields = null;
        _utcExpires = DateTime.MinValue;
        _isExpiresSet = false;
        _maxAge = TimeSpan.Zero;
        _isMaxAgeSet = false;
        _proxyMaxAge = TimeSpan.Zero;
        _isProxyMaxAgeSet = false;
        _slidingExpiration = -1;
        _slidingDelta = TimeSpan.Zero;
        UtcTimestampCreated = DateTime.MinValue;
        _utcTimestampRequest = DateTime.MinValue;
        _validUntilExpires = -1;
        _allowInHistory = -1;
        _revalidation = HttpCacheRevalidation.None;
        _utcLastModified = DateTime.MinValue;
        _isLastModifiedSet = false;
        _etag = null;

        _generateLastModifiedFromFiles = false;
        _generateEtagFromFiles = false;
        _validationCallbackInfo = null;

        _useCachedHeaders = false;
        _headerCacheControl = default;
        _headerPragma = default;
        _headerExpires = default;
        _headerLastModified = default;
        _headerEtag = default;
        _headerVaryBy = default;

        _noMaxAgeInCacheControl = false;

        _omitVaryStar = -1;
    }

    public bool IsModified() => _isModified || VaryByContentEncodings.IsModified() || VaryByHeaders.IsModified() || VaryByParams.IsModified();

    private void Dirtied()
    {
        _isModified = true;
        _useCachedHeaders = false;
    }

    static internal void AppendValueToHeader(StringBuilder s, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            if (s.Length > 0)
            {
                s.Append(", ");
            }

            s.Append(value);
        }
    }

    private static readonly string?[] s_cacheabilityTokens = new string?[]
    {
            null,           // no enum
            "no-cache",     // HttpCacheability.NoCache
            "private",      // HttpCacheability.Private
            "no-cache",     // HttpCacheability.ServerAndNoCache
            "public",       // HttpCacheability.Public
            "private",      // HttpCacheability.ServerAndPrivate
            null            // None - not specified
    };

    private static readonly string?[] s_revalidationTokens = new string?[]
    {
            null,               // no enum
            "must-revalidate",  // HttpCacheRevalidation.AllCaches
            "proxy-revalidate", // HttpCacheRevalidation.ProxyCaches
            null                // HttpCacheRevalidation.None
    };

    private static readonly int[] s_cacheabilityValues = new int[]
    {
            -1,     // no enum
            0,      // HttpCacheability.NoCache
            2,      // HttpCacheability.Private
            1,      // HttpCacheability.ServerAndNoCache
            4,      // HttpCacheability.Public
            3,      // HttpCacheability.ServerAndPrivate
            100,    // None - though private by default, an explicit set will override
    };

    private void UpdateCachedHeaders(DateTime timestamp)
    {
        StringBuilder sb;
        HttpCacheability cacheability;
        int i, n;
        string? expirationDate;
        string? lastModifiedDate;
        string? varyByHeaders;
        bool omitVaryStar;

        if (_useCachedHeaders)
        {
            return;
        }

        _utcTimestampRequest = timestamp;

        //To enable Out of Band OutputCache Module support, we will always refresh the UtcTimestampRequest.
        if (UtcTimestampCreated == DateTime.MinValue)
        {
            UtcTimestampCreated = _utcTimestampRequest;
        }

        if (_slidingExpiration != 1)
        {
            _slidingDelta = TimeSpan.Zero;
        }
        else if (_isMaxAgeSet)
        {
            _slidingDelta = _maxAge;
        }
        else if (_isExpiresSet)
        {
            _slidingDelta = _utcExpires - UtcTimestampCreated;
        }
        else
        {
            _slidingDelta = TimeSpan.Zero;
        }

        _headerCacheControl = default;
        _headerPragma = default;
        _headerExpires = default;
        _headerLastModified = default;
        _headerEtag = default;
        _headerVaryBy = default;

        /*
         * Cache control header
         */
        sb = new StringBuilder();

        if (_cacheability == (HttpCacheability)(int)HttpCacheabilityLimits.None)
        {
            cacheability = HttpCacheability.Private;
        }
        else
        {
            cacheability = _cacheability;
        }

        AppendValueToHeader(sb, s_cacheabilityTokens[(int)cacheability]);

        if (cacheability == HttpCacheability.Public && _privateFields != null)
        {
            Debug.Assert(_privateFields.Size > 0);

            AppendValueToHeader(sb, "private=\"");
            sb.Append(_privateFields.GetKey(0));
            for (i = 1, n = _privateFields.Size; i < n; i++)
            {
                AppendValueToHeader(sb, _privateFields.GetKey(i));
            }

            sb.Append('\"');
        }

        if (cacheability != HttpCacheability.NoCache &&
                cacheability != HttpCacheability.ServerAndNoCache &&
                _noCacheFields != null)
        {

            Debug.Assert(_noCacheFields.Size > 0);

            AppendValueToHeader(sb, "no-cache=\"");
            sb.Append(_noCacheFields.GetKey(0));
            for (i = 1, n = _noCacheFields.Size; i < n; i++)
            {
                AppendValueToHeader(sb, _noCacheFields.GetKey(i));
            }

            sb.Append('\"');
        }

        if (_noStore)
        {
            AppendValueToHeader(sb, "no-store");
        }

        AppendValueToHeader(sb, s_revalidationTokens[(int)_revalidation]);

        if (_noTransforms)
        {
            AppendValueToHeader(sb, "no-transform");
        }

        if (_cacheExtension != null)
        {
            AppendValueToHeader(sb, _cacheExtension);
        }


        /*
         * don't send expiration information when item shouldn't be cached
         * for cached header, only add max-age when it doesn't change
         * based on the time requested
         */
        if (_slidingExpiration == 1
             && cacheability != HttpCacheability.NoCache
             && cacheability != HttpCacheability.ServerAndNoCache)
        {

            if (_isMaxAgeSet && !_noMaxAgeInCacheControl)
            {
                AppendValueToHeader(sb, "max-age=" + ((long)_maxAge.TotalSeconds).ToString(CultureInfo.InvariantCulture));
            }

            if (_isProxyMaxAgeSet && !_noMaxAgeInCacheControl)
            {
                AppendValueToHeader(sb, "s-maxage=" + ((long)(_proxyMaxAge).TotalSeconds).ToString(CultureInfo.InvariantCulture));
            }
        }

        if (sb.Length > 0)
        {
            _headerCacheControl = new HttpResponseHeader(HeaderNames.CacheControl, sb.ToString());
        }

        /*
         * Pragma: no-cache and Expires: -1
         */
        if (cacheability == HttpCacheability.NoCache || cacheability == HttpCacheability.ServerAndNoCache)
        {
            _headerPragma = s_headerPragmaNoCache;

            if (_allowInHistory != 1)
            {
                _headerExpires = s_headerExpiresMinus1;
            }
        }
        else
        {
            /*
             * Expires header.
             */
            if (_isExpiresSet && _slidingExpiration != 1)
            {
                expirationDate = FormatHttpDateTimeUtc(_utcExpires);
                _headerExpires = new HttpResponseHeader(HeaderNames.Expires, expirationDate);
            }

            /*
             * Last Modified header.
             */
            if (_isLastModifiedSet)
            {
                lastModifiedDate = FormatHttpDateTimeUtc(_utcLastModified);
                _headerLastModified = new HttpResponseHeader(HeaderNames.LastModified, lastModifiedDate);
            }


            if (cacheability != HttpCacheability.Private)
            {
                /*
                 * Etag.
                 */
                if (_etag != null)
                {
                    _headerEtag = new HttpResponseHeader(HeaderNames.ETag, _etag);
                }

                /*
                 * Vary
                 */
                varyByHeaders = null;

                // automatic VaryStar processing
                // See if anyone has explicitly set this value
                if (_omitVaryStar != -1)
                {
                    omitVaryStar = _omitVaryStar == 1 ? true : false;
                }
                else
                {
                    omitVaryStar = false;
                }

                if (!omitVaryStar)
                {
                    // Dev10 Bug 425047 - OutputCache Location="ServerAndClient" (HttpCacheability.ServerAndPrivate) should 
                    // not use "Vary: *" so the response can be cached on the client
                    if (_varyByCustom != null || (VaryByParams.IsModified() && !VaryByParams.IgnoreParams))
                    {
                        varyByHeaders = "*";
                    }
                }

                if (varyByHeaders == null)
                {
                    varyByHeaders = VaryByHeaders.ToHeaderString();
                }

                if (varyByHeaders != null)
                {
                    _headerVaryBy = new HttpResponseHeader(HeaderNames.Vary, varyByHeaders);
                }
            }
        }

        _useCachedHeaders = true;
    }

    private static string FormatHttpDateTimeUtc(DateTime dt) => dt.ToString("R", DateTimeFormatInfo.InvariantInfo);

    internal bool VerifyCallbacks(HttpContext context)
    {
        if (_validationCallbackInfo is { } callbacks)
        {
            var status = HttpValidationStatus.Valid;

            foreach (var callback in callbacks)
            {
                callback.handler(context, callback.data, ref status);

                if (status is not HttpValidationStatus.Valid)
                {
                    return false;
                }
            }
        }

        return true;
    }

    internal void AddHeaders(IHeaderDictionary headers, DateTime timestamp)
    {
        StringBuilder sb;
        string expirationDate;
        TimeSpan age, maxAge, proxyMaxAge;
        DateTime utcExpires;
        HttpResponseHeader? headerExpires;
        HttpResponseHeader? headerCacheControl;

        UpdateCachedHeaders(timestamp);
        headerExpires = _headerExpires;
        headerCacheControl = _headerCacheControl;

        /* 
         * reconstruct headers that vary with time 
         * don't send expiration information when item shouldn't be cached
         */
        if (_cacheability != HttpCacheability.NoCache && _cacheability != HttpCacheability.ServerAndNoCache)
        {
            if (_slidingExpiration == 1)
            {
                /* update Expires header */
                if (_isExpiresSet)
                {
                    utcExpires = _utcTimestampRequest + _slidingDelta;
                    expirationDate = FormatHttpDateTimeUtc(utcExpires);
                    headerExpires = new HttpResponseHeader(HeaderNames.Expires, expirationDate);
                }
            }
            else
            {
                if (_isMaxAgeSet || _isProxyMaxAgeSet)
                {
                    /* update max-age, s-maxage components of Cache-Control header */
                    if (headerCacheControl != null)
                    {
                        sb = new StringBuilder(headerCacheControl.Value);
                    }
                    else
                    {
                        sb = new StringBuilder();
                    }

                    age = _utcTimestampRequest - UtcTimestampCreated;
                    if (_isMaxAgeSet)
                    {
                        maxAge = _maxAge - age;
                        if (maxAge < TimeSpan.Zero)
                        {
                            maxAge = TimeSpan.Zero;
                        }

                        if (!_noMaxAgeInCacheControl)
                            AppendValueToHeader(sb, "max-age=" + ((long)maxAge.TotalSeconds).ToString(CultureInfo.InvariantCulture));
                    }

                    if (_isProxyMaxAgeSet)
                    {
                        proxyMaxAge = _proxyMaxAge - age;
                        if (proxyMaxAge < TimeSpan.Zero)
                        {
                            proxyMaxAge = TimeSpan.Zero;
                        }

                        if (!_noMaxAgeInCacheControl)
                            AppendValueToHeader(sb, "s-maxage=" + ((long)(proxyMaxAge).TotalSeconds).ToString(CultureInfo.InvariantCulture));
                    }

                    headerCacheControl = new HttpResponseHeader(HeaderNames.CacheControl, sb.ToString());
                }
            }
        }

        static void Add(IHeaderDictionary headers, HttpResponseHeader header) => headers.Append(header.HeaderName, header.Value);

        if (headerCacheControl != null)
        {
            Add(headers, headerCacheControl);
        }

        if (_headerPragma != null)
        {
            Add(headers, _headerPragma);
        }

        if (headerExpires != null)
        {
            Add(headers, headerExpires);
        }

        if (_headerLastModified != null)
        {
            Add(headers, _headerLastModified);
        }

        if (_headerEtag != null)
        {
            Add(headers, _headerEtag);
        }

        if (_headerVaryBy != null)
        {
            Add(headers, _headerVaryBy);
        }
    }

    public void SetNoServerCaching()
    {
        Dirtied();
        _noServerCaching = true;
    }

    public bool GetNoServerCaching() => _noServerCaching;

    public void SetVaryByCustom(string custom)
    {
        ArgumentNullException.ThrowIfNull(custom);

        if (_varyByCustom != null)
        {
            throw new InvalidOperationException("VaryByCustom already set");
        }

        Dirtied();
        _varyByCustom = custom;
    }

    public string? GetVaryByCustom() => _varyByCustom;

    public void AppendCacheExtension(string extension)
    {
        ArgumentNullException.ThrowIfNull(extension);

        Dirtied();
        if (_cacheExtension == null)
        {
            _cacheExtension = extension;
        }
        else
        {
            _cacheExtension = _cacheExtension + ", " + extension;
        }
    }

    public string? GetCacheExtensions() => _cacheExtension;

    public void SetNoTransforms()
    {
        Dirtied();
        _noTransforms = true;
    }

    public bool GetNoTransforms() => _noTransforms;

    public bool GetIgnoreRangeRequests() => _ignoreRangeRequests;

    public HttpCacheVaryByContentEncodings VaryByContentEncodings { get; }

    public HttpCacheVaryByHeaders VaryByHeaders { get; }

    public HttpCacheVaryByParams VaryByParams { get; }

    public void SetCacheability(HttpCacheability cacheability)
    {
        if ((int)cacheability < (int)HttpCacheabilityLimits.MinValue ||
            (int)HttpCacheabilityLimits.MaxValue < (int)cacheability)
        {

            throw new ArgumentOutOfRangeException(nameof(cacheability));
        }

        if (s_cacheabilityValues[(int)cacheability] < s_cacheabilityValues[(int)_cacheability])
        {
            Dirtied();
            _cacheability = cacheability;
        }
    }

    public HttpCacheability GetCacheability() => _cacheability;

    public void SetCacheability(HttpCacheability cacheability, string field)
    {
        ArgumentNullException.ThrowIfNull(field);

        switch (cacheability)
        {
            case HttpCacheability.Private:
                if (_privateFields == null)
                {
                    _privateFields = new HttpDictionary();
                }

                _privateFields.SetValue(field, field);

                break;

            case HttpCacheability.NoCache:
                if (_noCacheFields == null)
                {
                    _noCacheFields = new HttpDictionary();
                }

                _noCacheFields.SetValue(field, field);

                break;

            default:
                throw new ArgumentException("Cacheability for field must be private or nocache", nameof(cacheability));
        }

        Dirtied();
    }

    public void SetNoStore()
    {
        Dirtied();
        _noStore = true;
    }

    public bool GetNoStore() => _noStore;

    public void SetExpires(DateTime date)
    {
        DateTime utcDate, utcNow;

        utcDate = date.ToUniversalTime();
        utcNow = DateTime.UtcNow;

        if (utcDate - utcNow > s_oneYear)
        {
            utcDate = utcNow + s_oneYear;
        }

        if (!_isExpiresSet || utcDate < _utcExpires)
        {
            Dirtied();
            _utcExpires = utcDate;
            _isExpiresSet = true;
        }
    }

    public DateTime GetExpires() => _utcExpires;

    public void SetMaxAge(TimeSpan delta)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(delta, TimeSpan.Zero);

        if (s_oneYear < delta)
        {
            delta = s_oneYear;
        }

        if (!_isMaxAgeSet || delta < _maxAge)
        {
            Dirtied();
            _maxAge = delta;
            _isMaxAgeSet = true;
        }
    }

    public TimeSpan GetMaxAge() => _maxAge;

    public void SetProxyMaxAge(TimeSpan delta)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(delta, TimeSpan.Zero);

        if (!_isProxyMaxAgeSet || delta < _proxyMaxAge)
        {
            Dirtied();
            _proxyMaxAge = delta;
            _isProxyMaxAgeSet = true;
        }
    }

    public TimeSpan GetProxyMaxAge() => _proxyMaxAge;

    public void SetSlidingExpiration(bool slide)
    {
        if (_slidingExpiration == -1 || _slidingExpiration == 1)
        {
            Dirtied();
            _slidingExpiration = (slide) ? 1 : 0;
        }
    }

    public bool HasSlidingExpiration() => _slidingExpiration == 1;

    public void SetValidUntilExpires(bool validUntilExpires)
    {
        if (_validUntilExpires == -1 || _validUntilExpires == 1)
        {
            Dirtied();
            _validUntilExpires = (validUntilExpires) ? 1 : 0;
        }
    }

    public bool IsValidUntilExpires() => _validUntilExpires == 1;

    public void SetAllowResponseInBrowserHistory(bool allow)
    {
        if (_allowInHistory == -1 || _allowInHistory == 1)
        {
            Dirtied();
            _allowInHistory = (allow) ? 1 : 0;
        }
    }

    public void SetRevalidation(HttpCacheRevalidation revalidation)
    {
        if ((int)revalidation < (int)HttpCacheRevalidationLimits.MinValue ||
            (int)HttpCacheRevalidationLimits.MaxValue < (int)revalidation)
        {
            throw new ArgumentOutOfRangeException(nameof(revalidation));
        }

        if ((int)revalidation < (int)_revalidation)
        {
            Dirtied();
            _revalidation = revalidation;
        }
    }

    public HttpCacheRevalidation GetRevalidation() => _revalidation;

    public void SetETag(string etag)
    {
        ArgumentNullException.ThrowIfNull(etag);

        if (_etag != null)
        {
            throw new InvalidOperationException("Etag already set");
        }

        if (_generateEtagFromFiles)
        {
            throw new InvalidOperationException("Cannot both set and generate etag");
        }

        Dirtied();
        _etag = etag;
    }

    public string? GetETag() => _etag;

    public void SetLastModified(DateTime date)
    {
        var utcDate = date.ToUniversalTime();
        UtcSetLastModified(utcDate);
    }

    private void UtcSetLastModified(DateTime utcDate)
    {
        var utcNow = DateTime.UtcNow;
        if (utcDate > utcNow)
        {
            utcDate = utcNow;
        }

        /*
         * Because HTTP dates have a resolution of 1 second, we
         * need to store dates with 1 second resolution or comparisons
         * will be off.
         */

        utcDate = new DateTime(utcDate.Ticks - (utcDate.Ticks % TimeSpan.TicksPerSecond));
        if (!_isLastModifiedSet || utcDate > _utcLastModified)
        {
            Dirtied();
            _utcLastModified = utcDate;
            _isLastModifiedSet = true;
        }
    }

    public DateTime GetUtcLastModified() => _utcLastModified;

    public void SetLastModifiedFromFileDependencies()
    {
        Dirtied();
        _generateLastModifiedFromFiles = true;
    }

    public bool GetLastModifiedFromFileDependencies() => _generateLastModifiedFromFiles;

    public void SetETagFromFileDependencies()
    {
        if (_etag != null)
        {
            throw new InvalidOperationException("Cannot both set and generate etag");
        }

        Dirtied();
        _generateEtagFromFiles = true;
    }

    public bool GetETagFromFileDependencies() => _generateEtagFromFiles;

    public void SetOmitVaryStar(bool omit)
    {
        Dirtied();
        if (_omitVaryStar == -1 || _omitVaryStar == 1)
        {
            Dirtied();
            _omitVaryStar = (omit) ? 1 : 0;
        }
    }

    public int GetOmitVaryStar() => _omitVaryStar;

    public void AddValidationCallback(
            HttpCacheValidateHandler handler, object data)
    {
        ArgumentNullException.ThrowIfNull(handler);

        Dirtied();

        _validationCallbackInfo ??= new();
        _validationCallbackInfo.Add(new ValidationCallbackInfo(handler, data));
    }

    public DateTime UtcTimestampCreated { get; set; }

    private enum HttpCacheRevalidationLimits
    {
        MinValue = HttpCacheRevalidation.AllCaches,
        MaxValue = HttpCacheRevalidation.None
    }
}
