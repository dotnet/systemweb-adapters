// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;

namespace System.Web;

public class HttpServerUtilityWrapper : HttpServerUtilityBase
{
    private readonly HttpServerUtility _utility;

    public HttpServerUtilityWrapper(HttpServerUtility utility)
    {
        ArgumentNullException.ThrowIfNull(utility);

        _utility = utility;
    }

    public override void ClearError() => _utility.ClearError();

    public override Exception? GetLastError() => _utility.GetLastError();

    public override string MachineName => _utility.MachineName;

    public override string? HtmlEncode(string? value) => _utility.HtmlEncode(value);

    public override void HtmlEncode(string? value, TextWriter output) => _utility.HtmlEncode(value, output);

    public override string MapPath(string path) => _utility.MapPath(path);

    public override byte[]? UrlTokenDecode(string input) => HttpServerUtility.UrlTokenDecode(input);

    public override string UrlTokenEncode(byte[] input) => HttpServerUtility.UrlTokenEncode(input);
}
