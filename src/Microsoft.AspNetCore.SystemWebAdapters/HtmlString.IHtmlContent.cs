// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

using CoreHtmlString = Microsoft.AspNetCore.Html.HtmlString;

namespace System.Web;

public partial class HtmlString : IHtmlContent
{
    [SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = Constants.ApiFromAspNet)]
    void IHtmlContent.WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        writer.Write(_htmlString);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator HtmlString?(CoreHtmlString? other)
       => other is null ? null : new(other.Value ?? string.Empty);

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator CoreHtmlString?(HtmlString? other)
       => other is null ? null : new(other._htmlString);
}
#endif
