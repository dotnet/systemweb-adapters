// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.SystemWebAdapters.Utilities;

namespace System.Web.UI;

[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = Constants.ApiFromAspNet)]
[SuppressMessage("Naming", "CA1721:Property names should not match get methods", Justification = Constants.ApiFromAspNet)]
[SuppressMessage("Design", "CA1055:URI-like return values should not be strings", Justification = Constants.ApiFromAspNet)]
public class HtmlTextWriter : TextWriter
{
    public const char TagLeftChar = '<';
    public const char TagRightChar = '>';
    public const string SelfClosingChars = " /";
    public const string SelfClosingTagEnd = " />";
    public const string EndTagLeftChars = "</";
    public const char DoubleQuoteChar = '"';
    public const char SingleQuoteChar = '\'';
    public const char SpaceChar = ' ';
    public const char EqualsChar = '=';
    public const char SlashChar = '/';
    public const string EqualsDoubleQuoteString = "=\"";
    public const char SemicolonChar = ';';
    public const char StyleEqualsChar = ':';
    public const string DefaultTabString = "\t";

    // The DesignerRegion attribute name must be kept in sync with
    // System.Web.UI.Design.DesignerRegion.DesignerRegionNameAttribute
    internal const string DesignerRegionAttributeName = "_designerRegion";

    private static readonly Dictionary<string, HtmlTextWriterTag> _tagKeyLookupTable = new((int)HtmlTextWriterTag.Xml + 1, StringComparer.OrdinalIgnoreCase);
    private static readonly TagInformation[] _tagNameLookupArray = new TagInformation[(int)HtmlTextWriterTag.Xml + 1];
    private static readonly Dictionary<string, HtmlTextWriterAttribute> _attrKeyLookupTable = new((int)HtmlTextWriterAttribute.VCardName + 1);
    private static readonly AttributeInformation[] _attrNameLookupArray = new AttributeInformation[(int)HtmlTextWriterAttribute.VCardName + 1];

    private int _indentLevel;
    private bool _tabsPending;
    private readonly bool _isDescendant;
    private int _tagIndex;
    private HtmlTextWriterTag _tagKey;
    private string? _tagName;

    private readonly Stack<TagStackEntry> _endTags = new();
    private readonly List<RenderStyle> _styleList = new();
    private readonly List<RenderAttribute> _attrList = new();
    private readonly string _tabString;

    static HtmlTextWriter()
    {
        RegisterTag(string.Empty, HtmlTextWriterTag.Unknown, TagType.Other);
        RegisterTag("a", HtmlTextWriterTag.A, TagType.Inline);
        RegisterTag("acronym", HtmlTextWriterTag.Acronym, TagType.Inline);
        RegisterTag("address", HtmlTextWriterTag.Address, TagType.Other);
        RegisterTag("area", HtmlTextWriterTag.Area, TagType.NonClosing);
        RegisterTag("b", HtmlTextWriterTag.B, TagType.Inline);
        RegisterTag("base", HtmlTextWriterTag.Base, TagType.NonClosing);
        RegisterTag("basefont", HtmlTextWriterTag.Basefont, TagType.NonClosing);
        RegisterTag("bdo", HtmlTextWriterTag.Bdo, TagType.Inline);
        RegisterTag("bgsound", HtmlTextWriterTag.Bgsound, TagType.NonClosing);
        RegisterTag("big", HtmlTextWriterTag.Big, TagType.Inline);
        RegisterTag("blockquote", HtmlTextWriterTag.Blockquote, TagType.Other);
        RegisterTag("body", HtmlTextWriterTag.Body, TagType.Other);
        RegisterTag("br", HtmlTextWriterTag.Br, TagType.NonClosing);
        RegisterTag("button", HtmlTextWriterTag.Button, TagType.Inline);
        RegisterTag("caption", HtmlTextWriterTag.Caption, TagType.Other);
        RegisterTag("center", HtmlTextWriterTag.Center, TagType.Other);
        RegisterTag("cite", HtmlTextWriterTag.Cite, TagType.Inline);
        RegisterTag("code", HtmlTextWriterTag.Code, TagType.Inline);
        RegisterTag("col", HtmlTextWriterTag.Col, TagType.NonClosing);
        RegisterTag("colgroup", HtmlTextWriterTag.Colgroup, TagType.Other);
        RegisterTag("del", HtmlTextWriterTag.Del, TagType.Inline);
        RegisterTag("dd", HtmlTextWriterTag.Dd, TagType.Inline);
        RegisterTag("dfn", HtmlTextWriterTag.Dfn, TagType.Inline);
        RegisterTag("dir", HtmlTextWriterTag.Dir, TagType.Other);
        RegisterTag("div", HtmlTextWriterTag.Div, TagType.Other);
        RegisterTag("dl", HtmlTextWriterTag.Dl, TagType.Other);
        RegisterTag("dt", HtmlTextWriterTag.Dt, TagType.Inline);
        RegisterTag("em", HtmlTextWriterTag.Em, TagType.Inline);
        RegisterTag("embed", HtmlTextWriterTag.Embed, TagType.NonClosing);
        RegisterTag("fieldset", HtmlTextWriterTag.Fieldset, TagType.Other);
        RegisterTag("font", HtmlTextWriterTag.Font, TagType.Inline);
        RegisterTag("form", HtmlTextWriterTag.Form, TagType.Other);
        RegisterTag("frame", HtmlTextWriterTag.Frame, TagType.NonClosing);
        RegisterTag("frameset", HtmlTextWriterTag.Frameset, TagType.Other);
        RegisterTag("h1", HtmlTextWriterTag.H1, TagType.Other);
        RegisterTag("h2", HtmlTextWriterTag.H2, TagType.Other);
        RegisterTag("h3", HtmlTextWriterTag.H3, TagType.Other);
        RegisterTag("h4", HtmlTextWriterTag.H4, TagType.Other);
        RegisterTag("h5", HtmlTextWriterTag.H5, TagType.Other);
        RegisterTag("h6", HtmlTextWriterTag.H6, TagType.Other);
        RegisterTag("head", HtmlTextWriterTag.Head, TagType.Other);
        RegisterTag("hr", HtmlTextWriterTag.Hr, TagType.NonClosing);
        RegisterTag("html", HtmlTextWriterTag.Html, TagType.Other);
        RegisterTag("i", HtmlTextWriterTag.I, TagType.Inline);
        RegisterTag("iframe", HtmlTextWriterTag.Iframe, TagType.Other);
        RegisterTag("img", HtmlTextWriterTag.Img, TagType.NonClosing);
        RegisterTag("input", HtmlTextWriterTag.Input, TagType.NonClosing);
        RegisterTag("ins", HtmlTextWriterTag.Ins, TagType.Inline);
        RegisterTag("isindex", HtmlTextWriterTag.Isindex, TagType.NonClosing);
        RegisterTag("kbd", HtmlTextWriterTag.Kbd, TagType.Inline);
        RegisterTag("label", HtmlTextWriterTag.Label, TagType.Inline);
        RegisterTag("legend", HtmlTextWriterTag.Legend, TagType.Other);
        RegisterTag("li", HtmlTextWriterTag.Li, TagType.Inline);
        RegisterTag("link", HtmlTextWriterTag.Link, TagType.NonClosing);
        RegisterTag("map", HtmlTextWriterTag.Map, TagType.Other);
        RegisterTag("marquee", HtmlTextWriterTag.Marquee, TagType.Other);
        RegisterTag("menu", HtmlTextWriterTag.Menu, TagType.Other);
        RegisterTag("meta", HtmlTextWriterTag.Meta, TagType.NonClosing);
        RegisterTag("nobr", HtmlTextWriterTag.Nobr, TagType.Inline);
        RegisterTag("noframes", HtmlTextWriterTag.Noframes, TagType.Other);
        RegisterTag("noscript", HtmlTextWriterTag.Noscript, TagType.Other);
        RegisterTag("object", HtmlTextWriterTag.Object, TagType.Other);
        RegisterTag("ol", HtmlTextWriterTag.Ol, TagType.Other);
        RegisterTag("option", HtmlTextWriterTag.Option, TagType.Other);
        RegisterTag("p", HtmlTextWriterTag.P, TagType.Inline);
        RegisterTag("param", HtmlTextWriterTag.Param, TagType.Other);
        RegisterTag("pre", HtmlTextWriterTag.Pre, TagType.Other);
        RegisterTag("ruby", HtmlTextWriterTag.Ruby, TagType.Other);
        RegisterTag("rt", HtmlTextWriterTag.Rt, TagType.Other);
        RegisterTag("q", HtmlTextWriterTag.Q, TagType.Inline);
        RegisterTag("s", HtmlTextWriterTag.S, TagType.Inline);
        RegisterTag("samp", HtmlTextWriterTag.Samp, TagType.Inline);
        RegisterTag("script", HtmlTextWriterTag.Script, TagType.Other);
        RegisterTag("select", HtmlTextWriterTag.Select, TagType.Other);
        RegisterTag("small", HtmlTextWriterTag.Small, TagType.Other);
        RegisterTag("span", HtmlTextWriterTag.Span, TagType.Inline);
        RegisterTag("strike", HtmlTextWriterTag.Strike, TagType.Inline);
        RegisterTag("strong", HtmlTextWriterTag.Strong, TagType.Inline);
        RegisterTag("style", HtmlTextWriterTag.Style, TagType.Other);
        RegisterTag("sub", HtmlTextWriterTag.Sub, TagType.Inline);
        RegisterTag("sup", HtmlTextWriterTag.Sup, TagType.Inline);
        RegisterTag("table", HtmlTextWriterTag.Table, TagType.Other);
        RegisterTag("tbody", HtmlTextWriterTag.Tbody, TagType.Other);
        RegisterTag("td", HtmlTextWriterTag.Td, TagType.Inline);
        RegisterTag("textarea", HtmlTextWriterTag.Textarea, TagType.Inline);
        RegisterTag("tfoot", HtmlTextWriterTag.Tfoot, TagType.Other);
        RegisterTag("th", HtmlTextWriterTag.Th, TagType.Inline);
        RegisterTag("thead", HtmlTextWriterTag.Thead, TagType.Other);
        RegisterTag("title", HtmlTextWriterTag.Title, TagType.Other);
        RegisterTag("tr", HtmlTextWriterTag.Tr, TagType.Other);
        RegisterTag("tt", HtmlTextWriterTag.Tt, TagType.Inline);
        RegisterTag("u", HtmlTextWriterTag.U, TagType.Inline);
        RegisterTag("ul", HtmlTextWriterTag.Ul, TagType.Other);
        RegisterTag("var", HtmlTextWriterTag.Var, TagType.Inline);
        RegisterTag("wbr", HtmlTextWriterTag.Wbr, TagType.NonClosing);
        RegisterTag("xml", HtmlTextWriterTag.Xml, TagType.Other);

        RegisterAttribute("abbr", HtmlTextWriterAttribute.Abbr, true);
        RegisterAttribute("accesskey", HtmlTextWriterAttribute.Accesskey, true);
        RegisterAttribute("align", HtmlTextWriterAttribute.Align, false);
        RegisterAttribute("alt", HtmlTextWriterAttribute.Alt, true);
        RegisterAttribute("autocomplete", HtmlTextWriterAttribute.AutoComplete, false);
        RegisterAttribute("axis", HtmlTextWriterAttribute.Axis, true);
        RegisterAttribute("background", HtmlTextWriterAttribute.Background, true, true);
        RegisterAttribute("bgcolor", HtmlTextWriterAttribute.Bgcolor, false);
        RegisterAttribute("border", HtmlTextWriterAttribute.Border, false);
        RegisterAttribute("bordercolor", HtmlTextWriterAttribute.Bordercolor, false);
        RegisterAttribute("cellpadding", HtmlTextWriterAttribute.Cellpadding, false);
        RegisterAttribute("cellspacing", HtmlTextWriterAttribute.Cellspacing, false);
        RegisterAttribute("checked", HtmlTextWriterAttribute.Checked, false);
        RegisterAttribute("class", HtmlTextWriterAttribute.Class, true);
        RegisterAttribute("cols", HtmlTextWriterAttribute.Cols, false);
        RegisterAttribute("colspan", HtmlTextWriterAttribute.Colspan, false);
        RegisterAttribute("content", HtmlTextWriterAttribute.Content, true);
        RegisterAttribute("coords", HtmlTextWriterAttribute.Coords, false);
        RegisterAttribute("dir", HtmlTextWriterAttribute.Dir, false);
        RegisterAttribute("disabled", HtmlTextWriterAttribute.Disabled, false);
        RegisterAttribute("for", HtmlTextWriterAttribute.For, false);
        RegisterAttribute("headers", HtmlTextWriterAttribute.Headers, true);
        RegisterAttribute("height", HtmlTextWriterAttribute.Height, false);
        RegisterAttribute("href", HtmlTextWriterAttribute.Href, true, true);
        RegisterAttribute("id", HtmlTextWriterAttribute.Id, false);
        RegisterAttribute("longdesc", HtmlTextWriterAttribute.Longdesc, true, true);
        RegisterAttribute("maxlength", HtmlTextWriterAttribute.Maxlength, false);
        RegisterAttribute("multiple", HtmlTextWriterAttribute.Multiple, false);
        RegisterAttribute("name", HtmlTextWriterAttribute.Name, false);
        RegisterAttribute("nowrap", HtmlTextWriterAttribute.Nowrap, false);
        RegisterAttribute("onclick", HtmlTextWriterAttribute.Onclick, true);
        RegisterAttribute("onchange", HtmlTextWriterAttribute.Onchange, true);
        RegisterAttribute("readonly", HtmlTextWriterAttribute.ReadOnly, false);
        RegisterAttribute("rel", HtmlTextWriterAttribute.Rel, false);
        RegisterAttribute("rows", HtmlTextWriterAttribute.Rows, false);
        RegisterAttribute("rowspan", HtmlTextWriterAttribute.Rowspan, false);
        RegisterAttribute("rules", HtmlTextWriterAttribute.Rules, false);
        RegisterAttribute("scope", HtmlTextWriterAttribute.Scope, false);
        RegisterAttribute("selected", HtmlTextWriterAttribute.Selected, false);
        RegisterAttribute("shape", HtmlTextWriterAttribute.Shape, false);
        RegisterAttribute("size", HtmlTextWriterAttribute.Size, false);
        RegisterAttribute("src", HtmlTextWriterAttribute.Src, true, true);
        RegisterAttribute("style", HtmlTextWriterAttribute.Style, false);
        RegisterAttribute("tabindex", HtmlTextWriterAttribute.Tabindex, false);
        RegisterAttribute("target", HtmlTextWriterAttribute.Target, false);
        RegisterAttribute("title", HtmlTextWriterAttribute.Title, true);
        RegisterAttribute("type", HtmlTextWriterAttribute.Type, false);
        RegisterAttribute("usemap", HtmlTextWriterAttribute.Usemap, false);
        RegisterAttribute("valign", HtmlTextWriterAttribute.Valign, false);
        RegisterAttribute("value", HtmlTextWriterAttribute.Value, true);
        RegisterAttribute("vcard_name", HtmlTextWriterAttribute.VCardName, false);
        RegisterAttribute("width", HtmlTextWriterAttribute.Width, false);
        RegisterAttribute("wrap", HtmlTextWriterAttribute.Wrap, false);
        RegisterAttribute(DesignerRegionAttributeName, HtmlTextWriterAttribute.DesignerRegion, false);
    }

    public virtual bool IsValidFormAttribute(string attribute)
    {
        return true;
    }

    public override Encoding Encoding => InnerWriter.Encoding;

    [AllowNull]
    public override string NewLine
    {
        get
        {
            return InnerWriter.NewLine;
        }

        set
        {
            InnerWriter.NewLine = value;
        }
    }

    public int Indent
    {
        get
        {
            return _indentLevel;
        }
        set
        {
            Debug.Assert(value >= 0, "Bogus Indent... probably caused by mismatched Indent++ and Indent--");
            if (value < 0)
            {
                value = 0;
            }
            _indentLevel = value;
        }
    }

    //Gets or sets the TextWriter to use.
    public TextWriter InnerWriter { get; set; }

    public virtual void BeginRender()
    {
    }

    //Closes the document being written to.
    public override void Close()
    {
        InnerWriter.Close();
    }

    public virtual void EndRender()
    {
    }

    public override void Flush()
    {
        InnerWriter.Flush();
    }

    protected virtual void OutputTabs()
    {
        if (_tabsPending)
        {
            for (var i = 0; i < _indentLevel; i++)
            {
                InnerWriter.Write(_tabString);
            }
            _tabsPending = false;
        }
    }

    public override void Write(string? value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(value);
    }

    public override void Write(bool value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(value);
    }

    public override void Write(char value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(value);
    }

    public override void Write(char[]? buffer)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(buffer);
    }

    public override void Write(char[] buffer, int index, int count)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(buffer, index, count);
    }

    public override void Write(double value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(value);
    }

    public override void Write(float value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(value);
    }

    public override void Write(int value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(value);
    }

    public override void Write(long value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(value);
    }

    public override void Write(object? value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(value);
    }

    public override void Write(string format, object? arg0)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(format, arg0);
    }

    public override void Write(string format, object? arg0, object? arg1)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(format, arg0, arg1);
    }

    public override void Write(string format, params object?[] arg)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(format, arg);
    }

    public void WriteLineNoTabs(string s)
    {
        InnerWriter.WriteLine(s);
        _tabsPending = true;
    }

    public override void WriteLine(string? value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.WriteLine(value);
        _tabsPending = true;
    }

    public override void WriteLine()
    {
        InnerWriter.WriteLine();
        _tabsPending = true;
    }

    public override void WriteLine(bool value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.WriteLine(value);
        _tabsPending = true;
    }

    public override void WriteLine(char value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.WriteLine(value);
        _tabsPending = true;
    }

    public override void WriteLine(char[]? buffer)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.WriteLine(buffer);
        _tabsPending = true;
    }

    public override void WriteLine(char[] buffer, int index, int count)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.WriteLine(buffer, index, count);
        _tabsPending = true;
    }

    public override void WriteLine(double value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.WriteLine(value);
        _tabsPending = true;
    }

    public override void WriteLine(float value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.WriteLine(value);
        _tabsPending = true;
    }

    public override void WriteLine(int value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.WriteLine(value);
        _tabsPending = true;
    }

    public override void WriteLine(long value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.WriteLine(value);
        _tabsPending = true;
    }

    public override void WriteLine(object? value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.WriteLine(value);
        _tabsPending = true;
    }

    public override void WriteLine(string format, object? arg0)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.WriteLine(format, arg0);
        _tabsPending = true;
    }

    public override void WriteLine(string format, object? arg0, object? arg1)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.WriteLine(format, arg0, arg1);
        _tabsPending = true;
    }

    public override void WriteLine(string format, params object?[] arg)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.WriteLine(format, arg);
        _tabsPending = true;
    }

    public override void WriteLine(uint value)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.WriteLine(value);
        _tabsPending = true;
    }

    protected static void RegisterTag(string name, HtmlTextWriterTag key)
    {
        ArgumentNullException.ThrowIfNull(name);

        RegisterTag(name, key, TagType.Other);
    }

    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Tags should be lower case")]
    private static void RegisterTag(string name, HtmlTextWriterTag key, TagType type)
    {
        _tagKeyLookupTable[name] = key;

        // Pre-resolve the end tag
        string? endTag = null;
        if (type != TagType.NonClosing && key != HtmlTextWriterTag.Unknown)
        {
            endTag = EndTagLeftChars + name.ToLowerInvariant() + TagRightChar.ToString(CultureInfo.InvariantCulture);
        }

        if ((int)key < _tagNameLookupArray.Length)
        {
            _tagNameLookupArray[(int)key] = new TagInformation(name, type, endTag);
        }
    }

    protected static void RegisterAttribute(string name, HtmlTextWriterAttribute key)
    {
        RegisterAttribute(name, key, false);
    }

    private static void RegisterAttribute(string name, HtmlTextWriterAttribute key, bool encode)
    {
        RegisterAttribute(name, key, encode, false);
    }

    private static void RegisterAttribute(string name, HtmlTextWriterAttribute key, bool encode, bool isUrl)
    {
        _attrKeyLookupTable[name] = key;

        if ((int)key < _attrNameLookupArray.Length)
        {
            _attrNameLookupArray[(int)key] = new AttributeInformation(name, encode, isUrl);
        }
    }

    protected static void RegisterStyle(string name, HtmlTextWriterStyle key)
    {
        CssTextWriter.RegisterAttribute(name, key);
    }

    public HtmlTextWriter(TextWriter writer) : this(writer, DefaultTabString)
    {
    }

    public HtmlTextWriter(TextWriter writer, string tabString)
        : base(CultureInfo.InvariantCulture)
    {
        InnerWriter = writer;

        _tabString = tabString;
        _indentLevel = 0;
        _tabsPending = false;
        _isDescendant = GetType() != typeof(HtmlTextWriter);
    }

    protected HtmlTextWriterTag TagKey
    {
        get
        {
            return _tagKey;
        }
        set
        {
            _tagIndex = (int)value;
            if (_tagIndex < 0 || _tagIndex >= _tagNameLookupArray.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _tagKey = value;

            // If explicitly setting to unknown, keep the old tag name. This allows a string tag
            // to be set without clobbering it if setting TagKey to itself.
            if (value != HtmlTextWriterTag.Unknown)
            {
                _tagName = _tagNameLookupArray[_tagIndex].Name;
            }
        }
    }

    protected string? TagName
    {
        get
        {
            return _tagName;
        }
        set
        {
            _tagName = value;
            _tagKey = GetTagKey(_tagName);
            _tagIndex = (int)_tagKey;
            Debug.Assert(_tagIndex >= 0 && _tagIndex < _tagNameLookupArray.Length);
        }
    }

    public virtual void AddAttribute(string name, string value)
    {
        var attributeKey = GetAttributeKey(name);
        value = EncodeAttributeValue(attributeKey, value);

        AddAttribute(name, value, attributeKey);
    }

    //do not fix this spelling error
    //believe it or not, it is a backwards breaking change for languages that 
    //support late binding with named parameters VB.Net
    public virtual void AddAttribute(string name, string value, bool fEndode)
    {
        value = EncodeAttributeValue(value, fEndode);
        AddAttribute(name, value, GetAttributeKey(name));
    }

    public virtual void AddAttribute(HtmlTextWriterAttribute key, string value)
    {
        var attributeIndex = (int)key;
        if (attributeIndex >= 0 && attributeIndex < _attrNameLookupArray.Length)
        {
            var info = _attrNameLookupArray[attributeIndex];
            AddAttribute(info.Name, value, key, info.Encode, info.IsUrl);
        }
    }

    public virtual void AddAttribute(HtmlTextWriterAttribute key, string value, bool fEncode)
    {
        var attributeIndex = (int)key;
        if (attributeIndex >= 0 && attributeIndex < _attrNameLookupArray.Length)
        {
            var info = _attrNameLookupArray[attributeIndex];
            AddAttribute(info.Name, value, key, fEncode, info.IsUrl);
        }
    }

    protected virtual void AddAttribute(string name, string value, HtmlTextWriterAttribute key)
    {
        AddAttribute(name, value, key, false, false);
    }

    private void AddAttribute(string name, string value, HtmlTextWriterAttribute key, bool encode, bool isUrl)
    {
        _attrList.Add(new RenderAttribute(name, value, key, encode, isUrl));
    }

    public virtual void AddStyleAttribute(string name, string value)
    {
        AddStyleAttribute(name, value, CssTextWriter.GetStyleKey(name));
    }

    public virtual void AddStyleAttribute(HtmlTextWriterStyle key, string value)
    {
        AddStyleAttribute(CssTextWriter.GetStyleName(key), value, key);
    }

    protected virtual void AddStyleAttribute(string name, string value, HtmlTextWriterStyle key)
    {
        var style = new RenderStyle(name, value, key);

        if (CssTextWriter.IsStyleEncoded(key))
        {
            // note that only css attributes in an inline style value need to be attribute encoded
            // since CssTextWriter is used to render both embedded stylesheets and style attributes
            // the attribute encoding is done here.
            style = style with { Value = HttpUtility.HtmlAttributeEncode(value) };
        }

        _styleList.Add(style);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = Constants.ApiFromAspNet)]
    [return: NotNullIfNotNull(nameof(value))]
    protected string? EncodeAttributeValue(string value, bool fEncode)
    {
        if (value == null)
        {
            return null;
        }

        return !fEncode ? value : HttpUtility.HtmlAttributeEncode(value);
    }

    [return: NotNullIfNotNull(nameof(value))]
    protected virtual string? EncodeAttributeValue(HtmlTextWriterAttribute attrKey, string value)
    {
        var encode = true;

        if (0 <= (int)attrKey && (int)attrKey < _attrNameLookupArray.Length)
        {
            encode = _attrNameLookupArray[(int)attrKey].Encode;
        }

        return EncodeAttributeValue(value, encode);
    }

    // This does minimal URL encoding by converting spaces in the url to "%20".
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = Constants.ApiFromAspNet)]
    protected string EncodeUrl(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        // VSWhidbey 454348: escaped spaces in UNC share paths don't work in IE, so
        // we're not going to encode if it's a share.
        return !IsUncSharePath(url) ? HttpUtility.UrlPathEncode(url) : url;

        static bool IsUncSharePath(string path)
        {
            // e.g \\server\share\foo or //server/share/foo
            return path.Length > 2 && IsDirectorySeparatorChar(path[0]) && IsDirectorySeparatorChar(path[1]);
        }

        static bool IsDirectorySeparatorChar(char ch)
        {
            return (ch == '\\' || ch == '/');
        }
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = Constants.ApiFromAspNet)]
    protected HtmlTextWriterAttribute GetAttributeKey(string attrName)
    {
        if (!string.IsNullOrEmpty(attrName))
        {
            if (_attrKeyLookupTable.TryGetValue(attrName, out var key))
            {
                return key;
            }
        }

        return (HtmlTextWriterAttribute)(-1);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = Constants.ApiFromAspNet)]
    protected string GetAttributeName(HtmlTextWriterAttribute attrKey)
    {
        return (int)attrKey >= 0 && (int)attrKey < _attrNameLookupArray.Length ? _attrNameLookupArray[(int)attrKey].Name : string.Empty;
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = Constants.ApiFromAspNet)]
    protected HtmlTextWriterStyle GetStyleKey(string styleName)
    {
        return CssTextWriter.GetStyleKey(styleName);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = Constants.ApiFromAspNet)]
    protected string GetStyleName(HtmlTextWriterStyle styleKey)
    {
        return CssTextWriter.GetStyleName(styleKey);
    }

    protected virtual HtmlTextWriterTag GetTagKey(string? tagName)
    {
        if (!string.IsNullOrEmpty(tagName))
        {
            if (_tagKeyLookupTable.TryGetValue(tagName, out var key))
            {
                return key;
            }
        }

        return HtmlTextWriterTag.Unknown;
    }

    protected virtual string GetTagName(HtmlTextWriterTag tagKey)
    {
        var tagIndex = (int)tagKey;
        return tagIndex >= 0 && tagIndex < _tagNameLookupArray.Length ? _tagNameLookupArray[tagIndex].Name : string.Empty;
    }

    protected bool IsAttributeDefined(HtmlTextWriterAttribute key)
    {
        foreach (var attr in _attrList)
        {
            if (attr.Key == key)
            {
                return true;
            }
        }
        return false;
    }

    protected bool IsAttributeDefined(HtmlTextWriterAttribute key, [MaybeNullWhen(false)] out string value)
    {
        value = null;
        foreach (var attr in _attrList)
        {
            if (attr.Key == key)
            {
                value = attr.Value;
                return true;
            }
        }
        return false;
    }

    protected bool IsStyleAttributeDefined(HtmlTextWriterStyle key)
    {
        foreach (var style in _styleList)
        {
            if (style.Key == key)
            {
                return true;
            }
        }
        return false;
    }

    protected bool IsStyleAttributeDefined(HtmlTextWriterStyle key, [MaybeNullWhen(false)] out string value)
    {
        value = null;
        foreach (var style in _styleList)
        {
            if (style.Key == key)
            {
                value = style.Value;
                return true;
            }
        }
        return false;
    }

    protected virtual bool OnAttributeRender(string name, string? value, HtmlTextWriterAttribute key)
    {
        return true;
    }

    protected virtual bool OnStyleAttributeRender(string name, string? value, HtmlTextWriterStyle key)
    {
        return true;
    }

    protected virtual bool OnTagRender(string? name, HtmlTextWriterTag key)
    {
        return true;
    }

    protected string? PopEndTag()
    {
        if (_endTags.Count == 0)
        {
            throw new InvalidOperationException("A PopEndTag was called without a corresponding PushEndTag.");
        }

        var endTag = _endTags.Pop();

        TagKey = endTag.Tag;

        return endTag.Text;
    }

    protected void PushEndTag(string? endTag)
    {
        _endTags.Push(new TagStackEntry(_tagKey, endTag));
    }

    protected virtual void FilterAttributes()
    {
        _styleList.RemoveAll(style => !OnStyleAttributeRender(style.Name, style.Value, style.Key));
        _attrList.RemoveAll(attr => !OnAttributeRender(attr.Name, attr.Value, attr.Key));
    }

    public virtual void RenderBeginTag(string tagName)
    {
        TagName = tagName;
        RenderBeginTag(_tagKey);
    }

    public virtual void RenderBeginTag(HtmlTextWriterTag tagKey)
    {
        TagKey = tagKey;
        var renderTag = true;

        if (_isDescendant)
        {
            renderTag = OnTagRender(_tagName, _tagKey);

            // Inherited renderers will be expecting to be able to filter any of the attributes at this point
            FilterAttributes();

            // write text before begin tag
            var textBeforeTag = RenderBeforeTag();
            if (textBeforeTag != null)
            {
                if (_tabsPending)
                {
                    OutputTabs();
                }
                InnerWriter.Write(textBeforeTag);
            }
        }

        // gather information about this tag.
        var tagInfo = _tagNameLookupArray[_tagIndex];
        var tagType = tagInfo.TagType;
        var renderEndTag = renderTag && (tagType != TagType.NonClosing);
        var endTag = renderEndTag ? tagInfo.ClosingTag : null;

        // write the begin tag
        if (renderTag)
        {
            if (_tabsPending)
            {
                OutputTabs();
            }
            InnerWriter.Write(TagLeftChar);
            InnerWriter.Write(_tagName);

            string? styleValue = null;

            foreach (var attr in _attrList)
            {
                if (attr.Key == HtmlTextWriterAttribute.Style)
                {
                    // append style attribute in with other styles
                    styleValue = attr.Value;
                }
                else
                {
                    InnerWriter.Write(SpaceChar);
                    InnerWriter.Write(attr.Name);
                    if (attr.Value != null)
                    {
                        InnerWriter.Write(EqualsDoubleQuoteString);

                        var attrValue = attr.Value;
                        if (attr.IsUrl)
                        {
                            if (attr.Key != HtmlTextWriterAttribute.Href || !attrValue.StartsWith("javascript:", StringComparison.Ordinal))
                            {
                                attrValue = EncodeUrl(attrValue);
                            }
                        }
                        if (attr.Encode)
                        {
                            WriteHtmlAttributeEncode(attrValue);
                        }
                        else
                        {
                            InnerWriter.Write(attrValue);
                        }
                        InnerWriter.Write(DoubleQuoteChar);
                    }
                }
            }

            if (_styleList.Count > 0 || styleValue != null)
            {
                InnerWriter.Write(SpaceChar);
                InnerWriter.Write("style");
                InnerWriter.Write(EqualsDoubleQuoteString);

                CssTextWriter.WriteAttributes(InnerWriter, _styleList);
                if (styleValue != null)
                {
                    InnerWriter.Write(styleValue);
                }
                InnerWriter.Write(DoubleQuoteChar);
            }

            if (tagType == TagType.NonClosing)
            {
                InnerWriter.Write(SelfClosingTagEnd);
            }
            else
            {
                InnerWriter.Write(TagRightChar);
            }
        }

        var textBeforeContent = RenderBeforeContent();
        if (textBeforeContent != null)
        {
            if (_tabsPending)
            {
                OutputTabs();
            }
            InnerWriter.Write(textBeforeContent);
        }

        // write text before the content
        if (renderEndTag)
        {
            if (tagType != TagType.Inline)
            {
                WriteLine();
                Indent++;
            }

            // Manually build end tags for unknown tag types.
            if (endTag == null)
            {
                endTag = EndTagLeftChars + _tagName + TagRightChar.ToString(CultureInfo.InvariantCulture);
            }
        }

        if (_isDescendant)
        {
            // append text after the tag
            var textAfterTag = RenderAfterTag();
            if (textAfterTag != null)
            {
                endTag = (endTag == null) ? textAfterTag : textAfterTag + endTag;
            }

            // build end content and push it on stack to write in RenderEndTag
            // prepend text after the content
            var textAfterContent = RenderAfterContent();
            if (textAfterContent != null)
            {
                endTag = (endTag == null) ? textAfterContent : textAfterContent + endTag;
            }
        }

        // push end tag onto stack
        PushEndTag(endTag);

        // flush attribute and style lists for next tag
        _attrList.Clear();
        _styleList.Clear();
    }

    public virtual void RenderEndTag()
    {
        var endTag = PopEndTag();

        if (endTag != null)
        {
            if (_tagNameLookupArray[_tagIndex].TagType == TagType.Inline)
            {
                // Never inject crlfs at end of inline tags.
                //
                Write(endTag);
            }
            else
            {
                // unindent if not an inline tag
                WriteLine();
                Indent--;
                Write(endTag);
            }
        }
    }

    protected virtual string? RenderBeforeTag()
    {
        return null;
    }

    protected virtual string? RenderBeforeContent()
    {
        return null;
    }

    protected virtual string? RenderAfterContent()
    {
        return null;
    }

    protected virtual string? RenderAfterTag()
    {
        return null;
    }

    public virtual void WriteAttribute(string name, string value)
    {
        WriteAttribute(name, value, false /*encode*/);
    }

    public virtual void WriteAttribute(string name, string value, bool fEncode)
    {
        InnerWriter.Write(SpaceChar);
        InnerWriter.Write(name);
        if (value != null)
        {
            InnerWriter.Write(EqualsDoubleQuoteString);
            if (fEncode)
            {
                WriteHtmlAttributeEncode(value);
            }
            else
            {
                InnerWriter.Write(value);
            }
            InnerWriter.Write(DoubleQuoteChar);
        }
    }

    public virtual void WriteBeginTag(string tagName)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(TagLeftChar);
        InnerWriter.Write(tagName);
    }

    public virtual void WriteBreak()
    {
        // Space between br and / is for improved html compatibility.  See XHTML 1.0 specification, section C.2.
        Write("<br />");
    }

    public virtual void WriteFullBeginTag(string tagName)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(TagLeftChar);
        InnerWriter.Write(tagName);
        InnerWriter.Write(TagRightChar);
    }

    public virtual void WriteEndTag(string tagName)
    {
        if (_tabsPending)
        {
            OutputTabs();
        }
        InnerWriter.Write(TagLeftChar);
        InnerWriter.Write(SlashChar);
        InnerWriter.Write(tagName);
        InnerWriter.Write(TagRightChar);
    }

    public virtual void WriteStyleAttribute(string name, string value)
    {
        WriteStyleAttribute(name, value, false /*encode*/);
    }

    public virtual void WriteStyleAttribute(string name, string value, bool fEncode)
    {
        InnerWriter.Write(name);
        InnerWriter.Write(StyleEqualsChar);
        if (fEncode)
        {
            WriteHtmlAttributeEncode(value);
        }
        else
        {
            InnerWriter.Write(value);
        }
        InnerWriter.Write(SemicolonChar);
    }

    public virtual void WriteEncodedUrl(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        var i = url.IndexOf('?', StringComparison.OrdinalIgnoreCase);
        if (i != -1)
        {
            WriteUrlEncodedString(url.Substring(0, i), false);
            Write(url.AsSpan(i));
        }
        else
        {
            WriteUrlEncodedString(url, false);
        }
    }

    public virtual void WriteEncodedUrlParameter(string urlText)
    {
        WriteUrlEncodedString(urlText, true);
    }

    public virtual void WriteEncodedText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        const char NBSP = '\u00A0';

        // When inner text is retrieved for a text control, &nbsp; is
        // decoded to 0x00A0 (code point for nbsp in Unicode).
        // HtmlEncode doesn't encode 0x00A0  to &nbsp;, we need to do it
        // manually here.
        var length = text.Length;
        var pos = 0;
        while (pos < length)
        {
            var nbsp = text.IndexOf(NBSP, pos);
            if (nbsp < 0)
            {
                HttpUtility.HtmlEncode(pos == 0 ? text : text.Substring(pos, length - pos), this);
                pos = length;
            }
            else
            {
                if (nbsp > pos)
                {
                    HttpUtility.HtmlEncode(text.Substring(pos, nbsp - pos), this);
                }
                Write("&nbsp;");
                pos = nbsp + 1;
            }
        }
    }

    protected void WriteUrlEncodedString(string text, bool argument)
    {
        ArgumentNullException.ThrowIfNull(text);

        var length = text.Length;
        for (var i = 0; i < length; i++)
        {
            var ch = text[i];
            if (HttpEncoderUtility.IsUrlSafeChar(ch))
            {
                Write(ch);
            }
            else if (!argument &&
                      (ch == '/' ||
                       ch == ':' ||
                       ch == '#' ||
                       ch == ','
                      )
                    )
            {
                Write(ch);
            }
            else if (ch == ' ' && argument)
            {
                Write('+');
            }
            // for chars that their code number is less than 128 and have
            // not been handled above
            else if ((ch & 0xff80) == 0)
            {
                Write('%');
                Write(HttpEncoderUtility.IntToHex((ch >> 4) & 0xf));
                Write(HttpEncoderUtility.IntToHex((ch) & 0xf));
            }
            else
            {
                // VSWhidbey 448625: For DBCS characters, use UTF8 encoding
                // which can be handled by IIS5 and above.
                Write(HttpUtility.UrlEncode(char.ToString(ch), Encoding.UTF8));
            }
        }
    }

    internal void WriteHtmlAttributeEncode(string s)
    {
        HttpUtility.HtmlAttributeEncode(s, InnerWriter);
    }

    private readonly record struct TagStackEntry(HtmlTextWriterTag Tag, string? Text);

    private readonly record struct RenderAttribute(string Name, string Value, HtmlTextWriterAttribute Key, bool Encode, bool IsUrl);

    private readonly record struct AttributeInformation(string Name, bool Encode, bool IsUrl);

    private readonly record struct TagInformation(string Name, TagType TagType, string? ClosingTag);

    private enum TagType
    {
        Inline,
        NonClosing,
        Other,
    }
}
