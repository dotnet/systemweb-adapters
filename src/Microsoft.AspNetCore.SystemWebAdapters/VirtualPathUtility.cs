// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace System.Web;

/// <summary>
/// Provides utility methods for common virtual path operations.
/// </summary>
/// <remarks>
/// Portions of this code are based on code originally Copyright (c) 1999 Microsoft Corporation
/// System.Web.Util.UrlPath code at https://github.com/microsoft/referencesource/blob/master/System.Web/Util/UrlPath.cs
/// System.Web.Util.StringUtil at https://github.com/microsoft/referencesource/blob/master/System.Web/Util/StringUtil.cs
/// These files are released under an MIT licence according to https://github.com/microsoft/referencesource#license
/// </remarks>
public static class VirtualPathUtility
{

    #region "Error strings"
    private const string Empty_path_has_no_directory = "Empty path has no directory.";
    #endregion

    /// <summary>Appends the literal slash mark (/) to the end of the virtual path, if one does not already exist.</summary>
    /// <returns>The modified virtual path.</returns>
    /// <param name="virtualPath">The virtual path to append the slash mark to.</param>
    [return: NotNullIfNotNull("virtualPath")]
    public static string? AppendTrailingSlash(string? virtualPath)
    {
        if (virtualPath == null) return null;

        var l = virtualPath.Length;
        if (l == 0) return virtualPath;

        if (virtualPath[l - 1] != '/')
            virtualPath += '/';

        return virtualPath;
    }

    /// <summary>Combines a base path and a relative path.</summary>
    /// <returns>The combined <paramref name="basePath" /> and <paramref name="relativePath" />.</returns>
    /// <param name="basePath">The base path.</param>
    /// <param name="relativePath">The relative path.</param>
    /// <exception cref="HttpException">
    ///   <paramref name="relativePath" /> is a physical path.-or-<paramref name="relativePath" /> includes one or more colons.</exception>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="relativePath" /> is null or an empty string.-or-<paramref name="basePath" /> is null or an empty string.</exception>
    public static string Combine(string basePath, string relativePath)
    {
        return Util.UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPath, basePath, relativePath);
    }

    /// <summary>Returns the directory portion of a virtual path.</summary>
    /// <returns>The directory referenced in the virtual path. </returns>
    /// <param name="virtualPath">The virtual path.</param>
    /// <exception cref="ArgumentException">
    ///   <paramref name="virtualPath" /> is not rooted. - or -<paramref name="virtualPath" /> is null or an empty string.</exception>
    public static string? GetDirectory(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath))
            throw new ArgumentNullException(nameof(virtualPath), Empty_path_has_no_directory);

        if (virtualPath[0] != '/' && virtualPath[0] != Util.UrlPath.AppRelativeCharacter)
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Util.UrlPath.Path_must_be_rooted, virtualPath), nameof(virtualPath));

        if ((virtualPath[0] == Util.UrlPath.AppRelativeCharacter && virtualPath.Length == 1) || virtualPath == Util.UrlPath.AppRelativeCharacterString) return "/";
        if (virtualPath.Length == 1) return null;

        var slashIndex = virtualPath.LastIndexOf('/');

        // This could happen if the input looks like "~abc"
        if (slashIndex < 0)
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Util.UrlPath.Path_must_be_rooted, virtualPath), nameof(virtualPath));

        return virtualPath[..(slashIndex + 1)];
    }

    /// <summary>Retrieves the extension of the file that is referenced in the virtual path.</summary>
    /// <returns>The file name extension string literal, including the period (.), null, or an empty string ("").</returns>
    /// <param name="virtualPath">The virtual path.</param>
    /// <exception cref="ArgumentException">
    ///   <paramref name="virtualPath" /> contains one or more characters that are not valid, as defined in <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
    public static string? GetExtension(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath)) throw new ArgumentNullException(nameof(virtualPath));

        return Util.UrlPath.GetExtension(virtualPath);
    }

    /// <summary>Retrieves the file name of the file that is referenced in the virtual path.</summary>
    /// <returns>The file name literal after the last directory character in <paramref name="virtualPath" />; otherwise, the last directory name, if the last character of <paramref name="virtualPath" /> is a directory or volume separator character.</returns>
    /// <param name="virtualPath">The virtual path. </param>
    /// <exception cref="ArgumentException">
    ///   <paramref name="virtualPath" /> contains one or more characters that are not valid, as defined in <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
    public static string? GetFileName(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath)) throw new ArgumentNullException(nameof(virtualPath));
        if (!IsAppRelative(virtualPath) && !Util.UrlPath.IsRooted(virtualPath)) throw new ArgumentException($"The relative virtual path '{virtualPath}' is not allowed here.", nameof(virtualPath));
        return Util.UrlPath.GetFileName(virtualPath);
    }

    /// <summary>Returns a Boolean value indicating whether the specified virtual path is absolute; that is, it starts with a literal slash mark (/).</summary>
    /// <returns>true if <paramref name="virtualPath" /> is an absolute path and is not null or an empty string (""); otherwise, false.</returns>
    /// <param name="virtualPath">The virtual path to check. </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="virtualPath" /> is null.</exception>
    public static bool IsAbsolute(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath)) throw new ArgumentNullException(nameof(virtualPath));
        return Util.UrlPath.IsRooted(virtualPath);
    }

    /// <summary>Returns a Boolean value indicating whether the specified virtual path is relative to the application.</summary>
    /// <returns>true if <paramref name="virtualPath" /> is relative to the application; otherwise, false.</returns>
    /// <param name="virtualPath">The virtual path to check. </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="virtualPath" /> is null.</exception>
    public static bool IsAppRelative(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath)) throw new ArgumentNullException(nameof(virtualPath));
        return Util.UrlPath.IsAppRelativePath(virtualPath);
    }

    /// <summary>Returns the relative virtual path from one virtual path containing the root operator (the tilde [~]) to another.</summary>
    /// <returns>The relative virtual path from <paramref name="fromPath" /> to <paramref name="toPath" />.</returns>
    /// <param name="fromPath">The starting virtual path to return the relative virtual path from.</param>
    /// <param name="toPath">The ending virtual path to return the relative virtual path to.</param>
    /// <exception cref="ArgumentException">
    ///   <paramref name="fromPath" /> is not rooted.- or -<paramref name="toPath" /> is not rooted.</exception>
    public static string MakeRelative(string fromPath, string toPath) => Util.UrlPath.MakeRelative(fromPath, toPath);

    /// <summary>Removes a trailing slash mark (/) from a virtual path.</summary>
    /// <returns>A virtual path without a trailing slash mark, if the virtual path is not already the root directory ("/"); otherwise, null.</returns>
    /// <param name="virtualPath">The virtual path to remove any trailing slash mark from. </param>
    public static string? RemoveTrailingSlash(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath)) return null;
        var l = virtualPath.Length;
        if (l <= 1 || virtualPath[l - 1] != '/') return virtualPath;
        return virtualPath[..(l - 1)];
    }

    /// <summary>Converts a virtual path to an application absolute path.</summary>
    /// <returns>The absolute path representation of the specified virtual path. </returns>
    /// <param name="virtualPath">The virtual path to convert to an application-relative path. </param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <paramref name="virtualPath" /> is not rooted. </exception>
    /// <exception cref="HttpException">A leading double period (..) is used to exit above the top directory.</exception>
    public static string ToAbsolute(string virtualPath)
    {
        if (Util.UrlPath.IsRooted(virtualPath)) return virtualPath;
        if (IsAppRelative(virtualPath)) return Util.UrlPath.ReduceVirtualPath(Util.UrlPath.MakeVirtualPathAppAbsolute(virtualPath));
        throw new ArgumentException($"The relative virtual path '{virtualPath}' is not allowed here.", nameof(virtualPath));
    }

    /// <summary>Converts a virtual path to an application absolute path using the specified application path.</summary>
    /// <returns>The absolute virtual path representation of <paramref name="virtualPath" />.</returns>
    /// <param name="virtualPath">The virtual path to convert to an application-relative path.</param>
    /// <param name="applicationPath">The application path to use to convert <paramref name="virtualPath" /> to a relative path.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <paramref name="applicationPath" /> is not rooted.</exception>
    /// <exception cref="HttpException">A leading double period (..) is used in the application path to exit above the top directory.</exception>
    public static string ToAbsolute(string virtualPath, string applicationPath)
    {
        if (string.IsNullOrEmpty(applicationPath)) throw new ArgumentNullException(nameof(applicationPath));
        if (!Util.UrlPath.IsRooted(applicationPath)) throw new ArgumentException($"The relative virtual path '{virtualPath}' is not allowed here.", nameof(applicationPath));
        if (Util.UrlPath.IsRooted(virtualPath)) return virtualPath;
        var appPath = AppendTrailingSlash(applicationPath);
        if (IsAppRelative(virtualPath)) return Util.UrlPath.ReduceVirtualPath(Util.UrlPath.MakeVirtualPathAppAbsolute(virtualPath, appPath));
        throw new ArgumentException($"The relative virtual path '{virtualPath}' is not allowed here.", nameof(virtualPath));
    }

    /// <summary>Converts a virtual path to an application-relative path using the application virtual path that is in the <see cref="P:System.Web.HttpRuntime.AppDomainAppVirtualPath" /> property. </summary>
    /// <returns>The application-relative path representation of <paramref name="virtualPath" />.</returns>
    /// <param name="virtualPath">The virtual path to convert to an application-relative path. </param>
    /// <exception cref="ArgumentException">
    ///   <paramref name="virtualPath" /> is null. </exception>
    public static string ToAppRelative(string virtualPath) => ToAppRelative(virtualPath, HttpRuntime.AppDomainAppVirtualPath);

    /// <summary>Converts a virtual path to an application-relative path using a specified application path.</summary>
    /// <returns>The application-relative path representation of <paramref name="virtualPath" />.</returns>
    /// <param name="virtualPath">The virtual path to convert to an application-relative path. </param>
    /// <param name="applicationPath">The application path to use to convert <paramref name="virtualPath" /> to a relative path. </param>
    public static string ToAppRelative(string virtualPath, string applicationPath)
    {
        ArgumentNullException.ThrowIfNull(virtualPath);

        var appPath = AppendTrailingSlash(applicationPath);

        var appPathLength = appPath.Length;
        var virtualPathLength = virtualPath.Length;

        // If virtualPath is the same as the app path, but without the ending slash,
        // treat it as if it were truly the app path (VSWhidbey 495949)
        if (virtualPathLength == appPathLength - 1)
        {
            if (Util.StringUtil.StringStartsWithIgnoreCase(appPath, virtualPath))
                return Util.UrlPath.AppRelativeCharacterString;
        }

        if (!Util.UrlPath.VirtualPathStartsWithVirtualPath(virtualPath, appPath))
            return virtualPath;

        // If they are the same, just return "~/"
        if (virtualPathLength == appPathLength)
            return Util.UrlPath.AppRelativeCharacterString;

        // Special case for apps rooted at the root:
        if (appPathLength == 1)
            return Util.UrlPath.AppRelativeCharacter + virtualPath;

        return Util.UrlPath.AppRelativeCharacter + virtualPath[(appPathLength - 1)..];
    }
}
