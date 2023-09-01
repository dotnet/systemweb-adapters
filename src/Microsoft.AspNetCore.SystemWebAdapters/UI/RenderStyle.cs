// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;

/// <summary>
/// Holds information about each style attribute that needs to be rendered.
/// This is used by the tag rendering API of HtmlTextWriter.
/// </summary>
internal readonly record struct RenderStyle(string Name, string Value, HtmlTextWriterStyle Key);
