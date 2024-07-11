// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

public class HttpCachePolicyWrapper : HttpCachePolicyBase
{
    private HttpCachePolicy _httpCachePolicy;

    public HttpCachePolicyWrapper(HttpCachePolicy httpCachePolicy)
    {
        if (httpCachePolicy == null)
        {
            ArgumentNullException.ThrowIfNull(httpCachePolicy);
        }
        _httpCachePolicy = httpCachePolicy;
    }


    public override HttpCacheVaryByHeaders VaryByHeaders => _httpCachePolicy.VaryByHeaders;

    public override void SetCacheability(HttpCacheability cacheability) => _httpCachePolicy.SetCacheability(cacheability);

    public override void SetExpires(DateTime date) => _httpCachePolicy.SetExpires(date);

    public override void SetLastModified(DateTime date) => _httpCachePolicy.SetLastModified(date);

    public override void SetMaxAge(TimeSpan delta) => _httpCachePolicy.SetMaxAge(delta);

    public override void SetOmitVaryStar(bool omit) => _httpCachePolicy.SetOmitVaryStar(omit);

}
