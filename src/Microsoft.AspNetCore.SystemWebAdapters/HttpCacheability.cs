// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1027:Mark enums with FlagsAttribute", Justification = Constants.ApiFromAspNet)]
[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1008:Enums should have zero value", Justification = Constants.ApiFromAspNet)]
public enum HttpCacheability
{
    NoCache = 1,

    Private,

    Server,

    ServerAndNoCache = Server,

    Public,

    ServerAndPrivate,
}
