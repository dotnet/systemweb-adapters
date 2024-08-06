// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace System.Web;

public class HttpCachePolicyBase
{
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
    public virtual HttpCacheVaryByHeaders VaryByHeaders => throw new NotImplementedException();

    public virtual void SetCacheability(HttpCacheability cacheability) => throw new NotImplementedException();

    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = Constants.ApiFromAspNet)]
    public virtual void SetLastModified(DateTime date) => throw new NotImplementedException();

    public virtual void SetMaxAge(TimeSpan delta) => throw new NotImplementedException();

    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = Constants.ApiFromAspNet)]
    public virtual void SetExpires(DateTime date) => throw new NotImplementedException();

    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = Constants.ApiFromAspNet)]
    public virtual void SetOmitVaryStar(bool omit) => throw new NotImplementedException();
}
