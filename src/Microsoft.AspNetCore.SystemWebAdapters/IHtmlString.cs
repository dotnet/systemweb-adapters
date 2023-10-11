// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// This is available in-box in .NET Framework and .NET 8+. We type forward to that when possible, otherwise, provide an implementation here for compat purposes.
#if NETSTANDARD || NET6_0

namespace System.Web;

public interface IHtmlString
{
    string ToHtmlString();
}

#else

[assembly:System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Web.IHtmlString))]

#endif
