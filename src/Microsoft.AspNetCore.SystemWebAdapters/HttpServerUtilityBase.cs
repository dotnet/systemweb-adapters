// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace System.Web;

[SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
public class HttpServerUtilityBase
{
    public virtual string MachineName => throw new NotImplementedException();

    public virtual Exception? GetLastError() => throw new NotImplementedException();

    public virtual byte[]? UrlTokenDecode(string input) => throw new NotImplementedException();

    public virtual void ClearError() => throw new NotImplementedException();

    [SuppressMessage("Design", "CA1055:URI-like return values should not be strings", Justification = Constants.ApiFromAspNet)]
    public virtual string UrlTokenEncode(byte[] input) => throw new NotImplementedException();
}
