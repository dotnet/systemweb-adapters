// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;

namespace System.Web.UI;

/// <summary>
/// Derived TextWriter that provides CSS rendering API.
/// </summary>
internal sealed class CssTextWriter
{
    private static readonly Dictionary<string, HtmlTextWriterStyle> attrKeyLookupTable = new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<AttributeInformation> attrNameLookupArray = new();

    [Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "<Pending>")]
    static CssTextWriter()
    {
        // register known style attributes, HtmlTextWriterStyle.Length
        RegisterAttribute("background-color", HtmlTextWriterStyle.BackgroundColor);
        RegisterAttribute("background-image", HtmlTextWriterStyle.BackgroundImage, true, true);
        RegisterAttribute("border-collapse", HtmlTextWriterStyle.BorderCollapse);
        RegisterAttribute("border-color", HtmlTextWriterStyle.BorderColor);
        RegisterAttribute("border-style", HtmlTextWriterStyle.BorderStyle);
        RegisterAttribute("border-width", HtmlTextWriterStyle.BorderWidth);
        RegisterAttribute("color", HtmlTextWriterStyle.Color);
        RegisterAttribute("cursor", HtmlTextWriterStyle.Cursor);
        RegisterAttribute("direction", HtmlTextWriterStyle.Direction);
        RegisterAttribute("display", HtmlTextWriterStyle.Display);
        RegisterAttribute("filter", HtmlTextWriterStyle.Filter);
        RegisterAttribute("font-family", HtmlTextWriterStyle.FontFamily, true);
        RegisterAttribute("font-size", HtmlTextWriterStyle.FontSize);
        RegisterAttribute("font-style", HtmlTextWriterStyle.FontStyle);
        RegisterAttribute("font-variant", HtmlTextWriterStyle.FontVariant);
        RegisterAttribute("font-weight", HtmlTextWriterStyle.FontWeight);
        RegisterAttribute("height", HtmlTextWriterStyle.Height);
        RegisterAttribute("left", HtmlTextWriterStyle.Left);
        RegisterAttribute("list-style-image", HtmlTextWriterStyle.ListStyleImage, true, true);
        RegisterAttribute("list-style-type", HtmlTextWriterStyle.ListStyleType);
        RegisterAttribute("margin", HtmlTextWriterStyle.Margin);
        RegisterAttribute("margin-bottom", HtmlTextWriterStyle.MarginBottom);
        RegisterAttribute("margin-left", HtmlTextWriterStyle.MarginLeft);
        RegisterAttribute("margin-right", HtmlTextWriterStyle.MarginRight);
        RegisterAttribute("margin-top", HtmlTextWriterStyle.MarginTop);
        RegisterAttribute("overflow-x", HtmlTextWriterStyle.OverflowX);
        RegisterAttribute("overflow-y", HtmlTextWriterStyle.OverflowY);
        RegisterAttribute("overflow", HtmlTextWriterStyle.Overflow);
        RegisterAttribute("padding", HtmlTextWriterStyle.Padding);
        RegisterAttribute("padding-bottom", HtmlTextWriterStyle.PaddingBottom);
        RegisterAttribute("padding-left", HtmlTextWriterStyle.PaddingLeft);
        RegisterAttribute("padding-right", HtmlTextWriterStyle.PaddingRight);
        RegisterAttribute("padding-top", HtmlTextWriterStyle.PaddingTop);
        RegisterAttribute("position", HtmlTextWriterStyle.Position);
        RegisterAttribute("text-align", HtmlTextWriterStyle.TextAlign);
        RegisterAttribute("text-decoration", HtmlTextWriterStyle.TextDecoration);
        RegisterAttribute("text-overflow", HtmlTextWriterStyle.TextOverflow);
        RegisterAttribute("top", HtmlTextWriterStyle.Top);
        RegisterAttribute("vertical-align", HtmlTextWriterStyle.VerticalAlign);
        RegisterAttribute("visibility", HtmlTextWriterStyle.Visibility);
        RegisterAttribute("width", HtmlTextWriterStyle.Width);
        RegisterAttribute("white-space", HtmlTextWriterStyle.WhiteSpace);
        RegisterAttribute("z-index", HtmlTextWriterStyle.ZIndex);
    }

    /// <summary>
    /// Returns the HtmlTextWriterStyle value for known style attributes.
    /// </summary>
    public static HtmlTextWriterStyle GetStyleKey(string styleName)
    {
        if (!string.IsNullOrEmpty(styleName))
        {
            if (attrKeyLookupTable.TryGetValue(styleName, out var key))
            {
                return key;
            }
        }

        return (HtmlTextWriterStyle)(-1);
    }

    /// <summary>
    /// Returns the name of the attribute corresponding to the specified HtmlTextWriterStyle value.
    /// </summary>
    public static string GetStyleName(HtmlTextWriterStyle styleKey)
    {
        return (int)styleKey >= 0 && (int)styleKey < attrNameLookupArray.Count ? attrNameLookupArray[(int)styleKey].Name : string.Empty;
    }

    /// <summary>
    /// Does the specified style key require attribute value encoding if the value is being
    /// rendered in a style attribute.
    /// </summary>
    public static bool IsStyleEncoded(HtmlTextWriterStyle styleKey)
    {
        return (int)styleKey >= 0 && (int)styleKey < attrNameLookupArray.Count ? attrNameLookupArray[(int)styleKey].Encode : true;
    }

    /// <internalonly/>
    /// <summary>
    /// Registers the specified style attribute to create a mapping between a string representation
    /// and the corresponding HtmlTextWriterStyle value.
    /// </summary>
    internal static void RegisterAttribute(string name, HtmlTextWriterStyle key)
    {
        RegisterAttribute(name, key, false, false);
    }

    /// <internalonly/>
    /// <summary>
    /// Registers the specified style attribute to create a mapping between a string representation
    /// and the corresponding HtmlTextWriterStyle value.
    /// </summary>
    internal static void RegisterAttribute(string name, HtmlTextWriterStyle key, bool encode)
    {
        RegisterAttribute(name, key, encode, false);
    }

    /// <internalonly/>
    /// <summary>
    /// Registers the specified style attribute to create a mapping between a string representation
    /// and the corresponding HtmlTextWriterStyle value.
    /// In addition, the mapping can include additional information about the attribute type
    /// such as whether it is a URL.
    /// </summary>
    internal static void RegisterAttribute(string name, HtmlTextWriterStyle key, bool encode, bool isUrl)
    {
        attrKeyLookupTable[name] = key;

        if ((int)key < attrNameLookupArray.Count)
        {
            attrNameLookupArray[(int)key] = new AttributeInformation(name, encode, isUrl);
        }
    }

    /// <summary>
    /// Render the specified style attribute into the specified TextWriter.
    /// This method contains all the logic for rendering a CSS name/value pair.
    /// </summary>
    private static void WriteAttribute(TextWriter writer, HtmlTextWriterStyle key, string name, string value)
    {
        writer.Write(name);
        writer.Write(':');

        bool isUrl = false;
        if (key != (HtmlTextWriterStyle)(-1))
        {
            isUrl = attrNameLookupArray[(int)key].IsUrl;
        }

        if (isUrl == false)
        {
            writer.Write(value);
        }
        else
        {
            WriteUrlAttribute(writer, value);
        }

        writer.Write(';');
    }

    /// <summary>
    /// Render the specified style attributes. This is used by HtmlTextWriter to render out all
    /// its collected style attributes.
    /// </summary>
    internal static void WriteAttributes(TextWriter writer, IEnumerable<RenderStyle> styles)
    {
        foreach (var style in styles)
        {
            WriteAttribute(writer, style.Key, style.Name, style.Value);
        }
    }

    /// <summary>
    /// Writes out the specified URL value with the appropriate encoding
    /// and url() syntax.
    /// internal for unit testing.
    /// </summary>
    internal static void WriteUrlAttribute(TextWriter writer, string url)
    {
        string urlValue = url;

        char[] quotes = new char[] { '\'', '"' };
        char? surroundingQuote = null;

        if (url.StartsWith("url(", StringComparison.OrdinalIgnoreCase))
        {
            int urlIndex = 4;
            int urlLength = url.Length - 4;
            if (url.EndsWith(')'))
            {
                urlLength--;
            }

            // extract out the actual URL value
            urlValue = url.Substring(urlIndex, urlLength).Trim();
        }

        // The CSS specification http://www.w3.org/TR/CSS2/syndata.html#uri says the ' and " characters are
        // optional for specifying the url values. 
        // And we do not want to pass them to UrlPathEncode if they are present.
        foreach (char quote in quotes)
        {
            if (urlValue.StartsWith(quote) && urlValue.EndsWith(quote))
            {
                urlValue = urlValue.Trim(quote);
                surroundingQuote = quote;
                break;
            }
        }

        // write out the "url(" prefix
        writer.Write("url(");
        if (surroundingQuote != null)
        {
            writer.Write(surroundingQuote);
        }

        writer.Write(HttpUtility.UrlPathEncode(urlValue));

        if (surroundingQuote != null)
        {
            writer.Write(surroundingQuote);
        }
        // write out the end of the "url()" syntax
        writer.Write(")");
    }

    /// <summary>
    /// Holds information about each registered style attribute.
    /// </summary>
    private readonly record struct AttributeInformation(string Name, bool Encode, bool IsUrl);
}
