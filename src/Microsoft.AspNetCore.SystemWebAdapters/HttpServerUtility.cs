// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web;

[Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = Constants.ApiFromAspNet)]
public class HttpServerUtility
{
    private readonly HttpContextCore _context;

    internal HttpServerUtility(HttpContextCore context)
    {
        _context = context;
    }

    public string MachineName => Environment.MachineName;

    public string MapPath(string? path)
    {
        var appPath = (string.IsNullOrEmpty(path) ? VirtualPathUtility.GetDirectory(_context.Request.Path) :
            VirtualPathUtility.Combine(
                VirtualPathUtility.GetDirectory(_context.Request.Path) ?? "/"
                , path));
        var rootPath = HttpRuntime.AppDomainAppPath;
        if (string.IsNullOrEmpty(appPath)) return rootPath;
        return System.IO.Path.Combine(rootPath,
            appPath[1..]
            .Replace('/', System.IO.Path.DirectorySeparatorChar))
            .TrimEnd(System.IO.Path.DirectorySeparatorChar);
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = Constants.ApiFromAspNet)]
    public Exception? GetLastError() => null;

    public void ClearError()
    {
    }

    /// <summary>
    /// This method is similar to <see cref="WebEncoders.Base64UrlDecode(string)"/> but handles the trailing character that <see cref="UrlTokenEncode(byte[])"/>
    /// appends to the string.
    /// </summary>
    /// <param name="input">Value to decode</param>
    /// <returns>Decoded value.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static byte[]? UrlTokenDecode(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Length == 0)
        {
            return Array.Empty<byte>();
        }

        // The number of padding chars is expected to be the final character of the string
        if (!IsDigit(input[^1]))
        {
            return null;
        }

        return WebEncoders.Base64UrlDecode(input, 0, input.Length - 1);

        static bool IsDigit(char c) => (uint)(c - '0') <= (uint)('9' - '0');
    }


    /// <summary>
    /// This method is similar to <see cref="WebEncoders.Base64UrlEncode(byte[])"/> but the resulting string includes an extra character
    /// that is the count of how many padding characters where removed from the Base64 encoded value.
    /// </summary>
    /// <param name="input">Value to encode</param>
    /// <returns>URL encoded value.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    [Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1055:URI-like return values should not be strings", Justification = Constants.ApiFromAspNet)]
    public static string UrlTokenEncode(byte[] input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Length == 0)
        {
            return string.Empty;
        }

        var encoded = WebEncoders.Base64UrlEncode(input);
        var padding = (encoded.Length % 4) switch
        {
            0 => 0,
            2 => 2,
            3 => 1,
            _ => throw new FormatException("Invalid input"),
        };

        return $"{encoded}{padding}";
    }
}
