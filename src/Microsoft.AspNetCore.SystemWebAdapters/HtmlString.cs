// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETFRAMEWORK
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Web.HtmlString))]
#else
namespace System.Web;

public partial class HtmlString : IHtmlString
{
    private readonly string _htmlString;

    public HtmlString(string value) => _htmlString = value;

    public string ToHtmlString() => _htmlString;

    public override string ToString() => _htmlString;
}
#endif
