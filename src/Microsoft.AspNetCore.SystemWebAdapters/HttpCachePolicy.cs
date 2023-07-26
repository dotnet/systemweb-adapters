// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Net.Http.Headers;

namespace System.Web;

[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = Constants.ApiFromAspNet)]
public class HttpCachePolicy
{
    private readonly HttpContextCore _context;

    private HttpCacheVaryByHeaders? _varyByHeaders;
    private TimeSpan? _maxAge;
    private HttpCacheability? _cacheability;
    private bool _omitVaryStar;

    internal HttpCachePolicy(HttpContextCore context)
    {
        _context = context;
    }

    internal void UpdateHeaders()
    {
        var headers = Headers;

        if (_varyByHeaders is { IsEmpty: false })
        {
            headers.Headers.Vary = new(_varyByHeaders.GetHeaders(_omitVaryStar));
        }

        if (headers.Headers.CacheControl.Count == 0 || ShouldUpdateCache)
        {
            var cacheControl = headers.CacheControl ?? new();
            UpdateCacheControl(cacheControl);
            headers.CacheControl = cacheControl;
        }
    }

    private bool ShouldUpdateCache => _maxAge.HasValue || _cacheability.HasValue;

    private void UpdateCacheControl(CacheControlHeaderValue cacheControl)
    {
        cacheControl.MaxAge = _maxAge;
        cacheControl.Public = GetCacheability() == HttpCacheability.Public;
        cacheControl.Private = GetCacheability() == HttpCacheability.Private;
        cacheControl.NoCache = GetCacheability() == HttpCacheability.NoCache;
    }

    private ResponseHeaders Headers => _context.GetAdapter().Response.TypedHeaders;

    public HttpCacheVaryByHeaders VaryByHeaders => _varyByHeaders ??= new();

    public HttpCacheability GetCacheability() => _cacheability ?? HttpCacheability.Private;

    public void SetCacheability(HttpCacheability cacheability) => _cacheability = cacheability;

    public void SetLastModified(DateTime date) => Headers.LastModified = date.ToUniversalTime();

    public void SetMaxAge(TimeSpan delta) => _maxAge = delta;

    public void SetExpires(DateTime date)
    {
        var utcDate = date.ToUniversalTime();
        var utcNow = DateTime.UtcNow;
        var oneYear = TimeSpan.FromDays(365);

        if (utcDate - utcNow > oneYear)
        {
            utcDate = utcNow + oneYear;
        }

        var current = Headers.Expires;

        if (!current.HasValue || utcDate < current)
        {
            Headers.Expires = utcDate;
        }
    }

    public void SetOmitVaryStar(bool omit) => _omitVaryStar = omit;
}
