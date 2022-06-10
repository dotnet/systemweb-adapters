// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
public class HttpBrowserCapabilitiesBase
{
    public virtual string? Browser => throw new NotImplementedException();

    public virtual string? Version => throw new NotImplementedException();

    public virtual int MajorVersion => throw new NotImplementedException();

    public virtual double MinorVersion => throw new NotImplementedException();

    public virtual string? Platform => throw new NotImplementedException();

    public virtual bool Crawler => throw new NotImplementedException();
}
