// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace System.Web;

[SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = Constants.ApiFromAspNet)]
public class HttpCachePolicyBase
{
    public virtual HttpCacheVaryByHeaders VaryByHeaders => throw new NotImplementedException();

    public virtual HttpCacheVaryByContentEncodings VaryByContentEncodings => throw new NotImplementedException();

    public virtual HttpCacheVaryByParams VaryByParams => throw new NotImplementedException();

    public virtual void SetCacheability(HttpCacheability cacheability) => throw new NotImplementedException();

    public virtual void SetCacheability(HttpCacheability cacheability, string field) => throw new NotImplementedException();

    public virtual void SetLastModified(DateTime date) => throw new NotImplementedException();

    public virtual void SetMaxAge(TimeSpan delta) => throw new NotImplementedException();

    public virtual void SetProxyMaxAge(TimeSpan delta) => throw new NotImplementedException();

    public virtual void SetExpires(DateTime date) => throw new NotImplementedException();

    public virtual void SetOmitVaryStar(bool omit) => throw new NotImplementedException();

    public virtual void SetETag(string etag) => throw new NotImplementedException();

    public virtual void SetVaryByCustom(string custom) => throw new NotImplementedException();

    public virtual void AppendCacheExtension(string extension) => throw new NotImplementedException();

    public virtual void SetNoStore() => throw new NotImplementedException();

    public virtual void SetNoServerCaching() => throw new NotImplementedException();

    public virtual void SetNoTransforms() => throw new NotImplementedException();

    public virtual void SetRevalidation(HttpCacheRevalidation revalidation) => throw new NotImplementedException();

    public virtual void SetSlidingExpiration(bool slide) => throw new NotImplementedException();

    public virtual void SetValidUntilExpires(bool validUntilExpires) => throw new NotImplementedException();

    public virtual void SetAllowResponseInBrowserHistory(bool allow) => throw new NotImplementedException();

    public virtual void SetLastModifiedFromFileDependencies() => throw new NotImplementedException();

    public virtual void SetETagFromFileDependencies() => throw new NotImplementedException();

    public virtual void AddValidationCallback(HttpCacheValidateHandler handler, object data) => throw new NotImplementedException();
}
