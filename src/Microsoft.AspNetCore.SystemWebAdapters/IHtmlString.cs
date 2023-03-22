using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace System.Web;

/// <summary>
/// Represents an HTML-encoded string that should not be encoded again.
/// </summary>
public class HtmlString : IHtmlString
{
    private readonly string _htmlString;

    public HtmlString(string value) => _htmlString = value;

    public string ToHtmlString() => _htmlString;

    public override string ToString() => _htmlString;

    public static implicit operator Microsoft.AspNetCore.Html.HtmlString(HtmlString html)
    {
        ArgumentNullException.ThrowIfNull(html);

        return new(html._htmlString);
    }

    public Microsoft.AspNetCore.Html.IHtmlContent ToHtmlContent()
        => new Microsoft.AspNetCore.Html.HtmlString(_htmlString);
}

/// <summary>
/// Represents an HTML-encoded string that should not be encoded again.
/// </summary>
public interface IHtmlString : IHtmlContent, IFormattable
{
    string ToHtmlString();

    void IHtmlContent.WriteTo(TextWriter writer, HtmlEncoder encoder) => writer.Write(ToHtmlString());

    string IFormattable.ToString(string? format, System.IFormatProvider? formatProvider) => ToHtmlString();
}
