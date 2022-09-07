// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace System.Web.Util;

/// <summary>
/// Provides utility methods for handling url and virtual paths.
/// </summary>
/// <remarks>
/// Portions of this code are based on code originally Copyright (c) 1999 Microsoft Corporation
/// System.Web.Util.UrlPath code at https://github.com/microsoft/referencesource/blob/master/System.Web/Util/UrlPath.cs
/// These files are released under an MIT licence according to https://github.com/microsoft/referencesource#license
/// </remarks>
internal static class UrlPath
{
    internal const char AppRelativeCharacter = '~';
    internal const string AppRelativeCharacterString = "~/";
    private const string Invalid_vpath = "'{0}' is not a valid virtual path.";
    private const string Physical_path_not_allowed = "'{0}' is a physical path, but a virtual path was expected.";
    private const string Cannot_exit_up_top_directory = "Cannot use a leading .. to exit above the top directory.";
    internal const string Path_must_be_rooted = "The virtual path '{0}' is not rooted.";

    internal static bool HasTrailingSlash(string virtualPath) => virtualPath[^1] == '/';

    internal static bool IsRooted(string basepath) => string.IsNullOrEmpty(basepath) || basepath[0] == '/' || basepath[0] == '\\';

    // Checks if virtual path contains a protocol, which is referred to as a scheme in the
    // URI spec.
    private static bool HasScheme(string virtualPath)
    {
        // URIs have the format <scheme>:<scheme-specific-path>, e.g. mailto:user@ms.com,
        // http://server/, nettcp://server/, etc.  The <scheme> cannot contain slashes.
        // The virtualPath passed to this method may be absolute or relative. Although
        // ':' is only allowed in the <scheme-specific-path> if it is encoded, the 
        // virtual path that we're receiving here may be decoded, so it is impossible
        // for us to determine if virtualPath has a scheme.  We will be conservative
        // and err on the side of assuming it has a scheme when we cannot tell for certain.
        // To do this, we first check for ':'.  If not found, then it doesn't have a scheme.
        // If ':' is found, then as long as we find a '/' before the ':', it cannot be
        // a scheme because schemes don't contain '/'.  Otherwise, we will assume it has a 
        // scheme.
        var indexOfColon = virtualPath.IndexOf(':');
        if (indexOfColon == -1)
            return false;
        var indexOfSlash = virtualPath.IndexOf('/');
        return indexOfSlash == -1 || indexOfColon < indexOfSlash;
    }

    private static bool IsDirectorySeparatorChar(char ch) => ch == '\\' || ch == '/';

    private static bool IsUncSharePath(string path)
    {
        // e.g \\server\share\foo or //server/share/foo
        if (path.Length > 2 && IsDirectorySeparatorChar(path[0]) && IsDirectorySeparatorChar(path[1]))
            return true;
        return false;
    }

    private static bool IsAbsolutePhysicalPath(string path)
    {
        if (path == null || path.Length < 3) return false;

        // e.g c:\foo
        if (path[1] == ':' && IsDirectorySeparatorChar(path[2])) return true;

        // e.g \\server\share\foo or //server/share/foo
        return IsUncSharePath(path);
    }

    internal static void CheckValidVirtualPath(string path)
    {

        // Check if it looks like a physical path (UNC shares and C:)
        if (IsAbsolutePhysicalPath(path))
        {
            throw new HttpException(string.Format(Physical_path_not_allowed, path));
        }

        // Virtual path can't have colons.
        var iqs = path.IndexOf('?');
        if (iqs >= 0)
        {
            path = path[..iqs];
        }
        if (HasScheme(path))
        {
            throw new HttpException(string.Format(Invalid_vpath, path));
        }
    }

    internal static string Combine(string appPath, string basepath, string relative)
    {
        string path;

        if (string.IsNullOrEmpty(relative))
        {
             throw new ArgumentNullException(nameof(relative));
        }
        if (string.IsNullOrEmpty(basepath))
        {
            throw new ArgumentNullException(nameof(basepath));
        }

        if (basepath[0] == AppRelativeCharacter && basepath.Length == 1)
        {
            // If it's "~", change it to "~/"
            basepath = AppRelativeCharacterString;
        }
        else
        {
            // If the base path includes a file name, get rid of it before combining
            var lastSlashIndex = basepath.LastIndexOf('/');
            Debug.Assert(lastSlashIndex >= 0);
            if (lastSlashIndex < basepath.Length - 1)
            {
                basepath = basepath[..(lastSlashIndex + 1)];
            }
        }

        // Make sure it's a virtual path (ASURT 73641)
        Util.UrlPath.CheckValidVirtualPath(relative);

        if (Util.UrlPath.IsRooted(relative))
        {
            path = relative;
        }
        else
        {
            // If the path is exactly "~", just return the app root path
            if (relative.Length == 1 && relative[0] == AppRelativeCharacter)
                return appPath;

            // If the relative path starts with "~/" or "~\", treat it as app root
            // relative (ASURT 68628)
            if (IsAppRelativePath(relative))
            {
                if (appPath.Length > 1)
                    path = string.Concat(appPath, "/", relative.AsSpan(2));
                else
                    path = string.Concat("/", relative.AsSpan(2));
            }
            else
            {
                path = SimpleCombine(basepath, relative);
            }
        }

        return Reduce(path);
    }

    // This simple version of combine should only be used when the relative
    // path is known to be relative.  It's more efficient, but doesn't do any
    // sanity checks.
    internal static string SimpleCombine(string basepath, string relative)
    {
        Debug.Assert(!string.IsNullOrEmpty(basepath));
        Debug.Assert(!string.IsNullOrEmpty(relative));
        Debug.Assert(relative[0] != '/');

        if (HasTrailingSlash(basepath))
            return basepath + relative;
        else
            return basepath + "/" + relative;
    }

    internal static string Reduce(string path)
    {
        // ignore query string
        string? queryString = null;
        if (!string.IsNullOrEmpty(path))
        {
            var iqs = path.IndexOf('?');
            if (iqs >= 0)
            {
                queryString = path[iqs..];
                path = path[..iqs];
            }
        }

        // Take care of backslashes and duplicate slashes
        path = FixVirtualPathSlashes(path);

        path = ReduceVirtualPath(path);

        return queryString != null ? path + queryString : path;
    }

    // Change backslashes to forward slashes, and remove duplicate slashes
    internal static string FixVirtualPathSlashes(string virtualPath)
    {
        // Make sure we don't have any back slashes
        virtualPath = virtualPath.Replace('\\', '/');

        // Replace any double forward slashes
        for (; ; )
        {
            var newPath = virtualPath.Replace("//", "/");

            // If it didn't do anything, we're done
            if (newPath == (object)virtualPath)
                break;

            // We need to loop again to take care of triple (or more) slashes (VSWhidbey 288782)
            virtualPath = newPath;
        }

        return virtualPath;
    }

    internal static string MakeVirtualPathAppAbsolute(string virtualPath)
    {
        return MakeVirtualPathAppAbsolute(virtualPath, HttpRuntime.AppDomainAppVirtualPath);
    }

    // If a virtual path is app relative (i.e. starts with ~/), change it to
    // start with the actuall app path.
    // E.g. ~/Sub/foo.aspx --> /MyApp/Sub/foo.aspx
    internal static string MakeVirtualPathAppAbsolute(string virtualPath, string applicationPath)
    {
        // If the path is exactly "~", just return the app root path
        if (virtualPath.Length == 1 && virtualPath[0] == AppRelativeCharacter)
            return applicationPath;

        // If the virtual path starts with "~/" or "~\", replace with the app path
        // relative (ASURT 68628)
        if (virtualPath.Length >= 2 && virtualPath[0] == AppRelativeCharacter &&
            (virtualPath[1] == '/' || virtualPath[1] == '\\'))
        {

            if (applicationPath.Length > 1)
            {
                Debug.Assert(HasTrailingSlash(applicationPath));
                return string.Concat(applicationPath, virtualPath.AsSpan(2));
            }
            else
                return string.Concat("/", virtualPath.AsSpan(2));
        }

        // Don't allow relative paths, since they cannot be made App Absolute
        if (!Util.UrlPath.IsRooted(virtualPath))
            throw new ArgumentOutOfRangeException(nameof(virtualPath));

        // Return it unchanged
        return virtualPath;
    }

    internal static bool VirtualPathStartsWithVirtualPath(string virtualPath1, string virtualPath2)
    {
        if (virtualPath1 == null)
        {
            throw new ArgumentNullException(nameof(virtualPath1));
        }

        if (virtualPath2 == null)
        {
            throw new ArgumentNullException(nameof(virtualPath2));
        }

        // if virtualPath1 as a string doesn't start with virtualPath2 as s string, then no for sure
        if (!StringUtil.StringStartsWithIgnoreCase(virtualPath1, virtualPath2))
        {
            return false;
        }

        var virtualPath2Length = virtualPath2.Length;

        // same length - same path
        if (virtualPath1.Length == virtualPath2Length)
        {
            return true;
        }

        // Special case for apps rooted at the root. VSWhidbey 286145
        if (virtualPath2Length == 1)
        {
            Debug.Assert(virtualPath2[0] == '/');
            return true;
        }

        // If virtualPath2 ends with a '/', it's definitely a child
        if (virtualPath2[virtualPath2Length - 1] == '/')
            return true;

        // If it doesn't, make sure the next char in virtualPath1 is a '/'.
        // e.g. /app1 vs /app11 (VSWhidbey 285038)
        if (virtualPath1[virtualPath2Length] != '/')
        {
            return false;
        }

        // passed all checks
        return true;
    }

    internal static bool IsAppRelativePath(string? virtualPath)
    {
        if (virtualPath is null) return false;
        var len = virtualPath.Length;

        // Empty string case
        if (len == 0) return false;

        // It must start with ~
        if (virtualPath[0] != AppRelativeCharacter) return false;

        // Single character case: "~"
        if (len == 1) return true;

        // If it's longer, checks if it starts with "~/" or "~\"
        return virtualPath[1] == '\\' || virtualPath[1] == '/';
    }

    // Same as Reduce, but for a virtual path that is known to be well formed
    internal static string ReduceVirtualPath(string path)
    {

        var length = path.Length;
        int examine;

        // quickly rule out situations in which there are no . or ..

        for (examine = 0; ; examine++)
        {
            examine = path.IndexOf('.', examine);
            if (examine < 0)
                return path;

            if ((examine == 0 || path[examine - 1] == '/')
                && (examine + 1 == length || path[examine + 1] == '/' ||
                    path[examine + 1] == '.' && (examine + 2 == length || path[examine + 2] == '/')))
                break;
        }

        // OK, we found a . or .. so process it:

        List<int> list = new();
        var sb = new StringBuilder();
        int start;
        examine = 0;

        for (; ; )
        {
            start = examine;
            examine = path.IndexOf('/', start + 1);

            if (examine < 0)
                examine = length;

            if (examine - start <= 3 &&
                (examine < 1 || path[examine - 1] == '.') &&
                (start + 1 >= length || path[start + 1] == '.'))
            {
                if (examine - start == 3)
                {
                    if (list.Count == 0)
                        throw new HttpException(Cannot_exit_up_top_directory);

                    // We're about to backtrack onto a starting '~', which would yield
                    // incorrect results.  Instead, make the path App Absolute, and call
                    // Reduce on that.
                    if (list.Count == 1 && IsAppRelativePath(path))
                    {
                        Debug.Assert(sb.Length == 1);
                        return ReduceVirtualPath(Util.UrlPath.MakeVirtualPathAppAbsolute(path));
                    }

                    sb.Length = list[^1];
                    list.RemoveRange(list.Count - 1, 1);
                }
            }
            else
            {
                list.Add(sb.Length);

                sb.Append(path, start, examine - start);
            }

            if (examine == length)
                break;
        }

        var result = sb.ToString();

        // If we end up with en empty string, turn it into either "/" or "." (VSWhidbey 289175)
        if (result.Length == 0)
        {
            if (length > 0 && path[0] == '/')
                result = @"/";
            else
                result = ".";
        }

        return result;
    }

    // We use file: protocol instead of http:, so that Uri.MakeRelative behaves
    // in a case insensitive way (VSWhidbey 80078)
    private const string dummyProtocolAndServer = "file://foo";
    private static readonly char[] s_slashChars = new char[] { '\\', '/' };

    internal static string MakeRelative(string fromPath, string toPath)
    {
        // If either path is app relative (~/...), make it absolute, since the Uri
        // class wouldn't know how to deal with it.
        fromPath = Util.UrlPath.MakeVirtualPathAppAbsolute(fromPath);
        toPath = Util.UrlPath.MakeVirtualPathAppAbsolute(toPath);

        // Make sure both virtual paths are rooted
        if (!Util.UrlPath.IsRooted(fromPath))
            throw new ArgumentException(string.Format(Path_must_be_rooted, fromPath));
        if (!Util.UrlPath.IsRooted(toPath))
            throw new ArgumentException(string.Format(Path_must_be_rooted, toPath));

        // Remove the query string, so that System.Uri doesn't corrupt it
        string? queryString = null;
        if (!string.IsNullOrEmpty(toPath))
        {
            var iqs = toPath.IndexOf('?');
            if (iqs >= 0)
            {
                queryString = toPath[iqs..];
                toPath = toPath[..iqs];
            }
        }

        // Uri's need full url's so, we use a dummy root
        Uri fromUri = new(dummyProtocolAndServer + fromPath);
        Uri toUri = new(dummyProtocolAndServer + toPath);

        string relativePath;

        // VSWhidbey 144946: If to and from points to identical path (excluding query and fragment), just use them instead
        // of returning an empty string.
        if (fromUri.Equals(toUri))
        {
            var iPos = toPath.LastIndexOfAny(s_slashChars);

            if (iPos >= 0)
            {
                // If it's the same directory, simply return "./"
                // Browsers should interpret "./" as the original directory.
                if (iPos == toPath.Length - 1)
                {
                    relativePath = "./";
                }
                else
                {
                    relativePath = toPath[(iPos + 1)..];
                }
            }
            else
            {
                relativePath = toPath;
            }
        }
        else
        {
            // To avoid deprecation warning.  It says we should use MakeRelativeUri instead (which returns a Uri),
            // but we wouldn't gain anything from it.  The way we use MakeRelative is hacky anyway (fake protocol, ...),
            // and I don't want to take the chance of breaking something with this change.
#pragma warning disable 0618
            relativePath = fromUri.MakeRelative(toUri);
#pragma warning restore 0618
        }

        // Note that we need to re-append the query string and fragment (e.g. #anchor)
        return relativePath + queryString + toUri.Fragment;
    }

    internal static string? GetFileName(string virtualPath)
    {
        if (virtualPath is not null)
        {
            var length = virtualPath.Length;
            for (var i = length; --i >= 0;)
            {
                var ch = virtualPath[i];
                if (ch == '/')
                    return virtualPath.Substring(i + 1, length - i - 1);
            }
        }
        return virtualPath;
    }

    [return: NotNullIfNotNull("virtualPath")]
    internal static string? GetExtension(string? virtualPath)
    {
        if (virtualPath is null) return null;

        var length = virtualPath.Length;
        for (var i = length; --i >= 0;)
        {
            var ch = virtualPath[i];
            if (ch == '.')
            {
                if (i != length - 1)
                    return virtualPath[i..length];
                else
                    return string.Empty;
            }
            if (ch == '/')
                break;
        }
        return string.Empty;
    }
}
