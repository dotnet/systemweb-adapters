// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

public class HttpCachePolicyWrapper : HttpCachePolicyBase
{
    private readonly HttpCachePolicy _httpCachePolicy;

    public HttpCachePolicyWrapper(HttpCachePolicy httpCachePolicy)
    {
        ArgumentNullException.ThrowIfNull(httpCachePolicy);

        _httpCachePolicy = httpCachePolicy;
    }

    public override HttpCacheVaryByHeaders VaryByHeaders => _httpCachePolicy.VaryByHeaders;

    public override HttpCacheVaryByContentEncodings VaryByContentEncodings => _httpCachePolicy.VaryByContentEncodings;

    public override HttpCacheVaryByParams VaryByParams => _httpCachePolicy.VaryByParams;

    public override void SetCacheability(HttpCacheability cacheability) => _httpCachePolicy.SetCacheability(cacheability);

    public override void SetCacheability(HttpCacheability cacheability, string field) => _httpCachePolicy.SetCacheability(cacheability, field);

    public override void SetLastModified(DateTime date) => _httpCachePolicy.SetLastModified(date);

    public override void SetMaxAge(TimeSpan delta) => _httpCachePolicy.SetMaxAge(delta);

    public override void SetProxyMaxAge(TimeSpan delta) => _httpCachePolicy.SetProxyMaxAge(delta);

    public override void SetExpires(DateTime date) => _httpCachePolicy.SetExpires(date);

    public override void SetOmitVaryStar(bool omit) => _httpCachePolicy.SetOmitVaryStar(omit);

    public override void SetETag(string etag) => _httpCachePolicy.SetETag(etag);

    public override void SetVaryByCustom(string custom) => _httpCachePolicy.SetVaryByCustom(custom);

    public override void AppendCacheExtension(string extension) => _httpCachePolicy.AppendCacheExtension(extension);

    public override void SetNoStore() => _httpCachePolicy.SetNoStore();

    public override void SetNoServerCaching() => _httpCachePolicy.SetNoServerCaching();

    public override void SetNoTransforms() => _httpCachePolicy.SetNoTransforms();

    public override void SetRevalidation(HttpCacheRevalidation revalidation) => _httpCachePolicy.SetRevalidation(revalidation);

    public override void SetSlidingExpiration(bool slide) => _httpCachePolicy.SetSlidingExpiration(slide);

    public override void SetValidUntilExpires(bool validUntilExpires) => _httpCachePolicy.SetValidUntilExpires(validUntilExpires);

    public override void SetAllowResponseInBrowserHistory(bool allow) => _httpCachePolicy.SetAllowResponseInBrowserHistory(allow);

    public override void SetLastModifiedFromFileDependencies() => _httpCachePolicy.SetLastModifiedFromFileDependencies();

    public override void SetETagFromFileDependencies() => _httpCachePolicy.SetETagFromFileDependencies();
}
