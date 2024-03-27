// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;

namespace System.Web;

[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
public abstract class HttpPostedFileBase
{
    public virtual string FileName => throw new NotImplementedException();

    public virtual string ContentType => throw new NotImplementedException();

    public virtual int ContentLength => throw new NotImplementedException();

    public virtual Stream InputStream => throw new NotImplementedException();

    public virtual void SaveAs(string filename) => throw new NotImplementedException(); 
}
