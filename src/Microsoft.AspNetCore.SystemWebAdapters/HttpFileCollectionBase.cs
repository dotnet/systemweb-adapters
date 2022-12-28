// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Collections.Specialized;

namespace System.Web;

public abstract class HttpFileCollectionBase : NameObjectCollectionBase
{
    public virtual string[] AllKeys => throw new NotImplementedException();

    public virtual HttpPostedFileBase? this[string name] => throw new NotImplementedException();

    public virtual HttpPostedFileBase? this[int index] => throw new NotImplementedException();

    public virtual HttpPostedFileBase? Get(int index) => throw new NotImplementedException();

    public virtual HttpPostedFileBase? Get(string name) => throw new NotImplementedException();

    public virtual IList<HttpPostedFileBase> GetMultiple(string name) => throw new NotImplementedException();
}
