// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Collections.Specialized;

namespace System.Web;

[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = Constants.ApiFromAspNet)]
[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
[Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = Constants.ApiFromAspNet)]
public abstract class HttpFileCollectionBase : NameObjectCollectionBase
{
    [Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = Constants.ApiFromAspNet)]
    public virtual string[] AllKeys => throw new NotImplementedException();

    public virtual HttpPostedFileBase? this[string name] => throw new NotImplementedException();

    public virtual HttpPostedFileBase? this[int index] => throw new NotImplementedException();

    public virtual HttpPostedFileBase? Get(int index) => throw new NotImplementedException();

    public virtual HttpPostedFileBase? Get(string name) => throw new NotImplementedException();

    public virtual IList<HttpPostedFileBase> GetMultiple(string name) => throw new NotImplementedException();
}
