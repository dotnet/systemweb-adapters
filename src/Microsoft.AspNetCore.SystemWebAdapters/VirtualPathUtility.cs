using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

    #region "Private implementation stuff"
    private const char appRelativeCharacter = '~';
    private const string appRelativeCharacterString = "~/";
    private const string Cannot_exit_up_top_directory = "Cannot use a leading .. to exit above the top directory.";
    private const string Empty_path_has_no_directory = "Empty path has no directory.";
    private const string Invalid_vpath = "'{0}' is not a valid virtual path.";
    private const string Path_must_be_rooted = "The virtual path '{0}' is not rooted.";
    private const string Physical_path_not_allowed = "'{0}' is a physical path, but a virtual path was expected.";

    private static bool IsRooted(string basepath) => string.IsNullOrEmpty(basepath) || basepath[0] == '/' || basepath[0] == '\\';

    private static bool IsDirectorySeparatorChar(char ch) => ch == '\\' || ch == '/';

    private static bool IsAbsolutePhysicalPath(string path)
    {
        if (path == null || path.Length < 3) return false;

        // e.g c:\foo
        if (path[1] == ':' && IsDirectorySeparatorChar(path[2])) return true;

        // e.g \\server\share\foo or //server/share/foo
        return IsUncSharePath(path);
    }

    private static bool IsUncSharePath(string path)
    {
        // e.g \\server\share\foo or //server/share/foo
        if (path.Length > 2 && IsDirectorySeparatorChar(path[0]) && IsDirectorySeparatorChar(path[1]))
            return true;
        return false;
    }

    private static bool HasTrailingSlash(string virtualPath) => virtualPath[^1] == '/';

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

    #region "Combine"
    /// <summary>Combines a base path and a relative path.</summary>
    /// <returns>The combined <paramref name="basePath" /> and <paramref name="relativePath" />.</returns>
    /// <param name="basePath">The base path.</param>
    /// <param name="relativePath">The relative path.</param>
    /// <exception cref="T:System.Web.HttpException">
    ///   <paramref name="relativePath" /> is a physical path.-or-<paramref name="relativePath" /> includes one or more colons.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="relativePath" /> is null or an empty string.-or-<paramref name="basePath" /> is null or an empty string.</exception>
    public static string Combine(string basePath, string relativePath)
    {
        return Combine(HttpRuntime.AppDomainAppVirtualPath, basePath, relativePath);
    }

    private static string Combine(string appPath, string basepath, string relative)
    {
        string path;

        if (string.IsNullOrEmpty(relative))
            throw new ArgumentNullException(nameof(relative));
        if (string.IsNullOrEmpty(basepath))
            throw new ArgumentNullException(nameof(basepath));

        if (basepath[0] == appRelativeCharacter && basepath.Length == 1)
        {
            // If it's "~", change it to "~/"
            basepath = appRelativeCharacterString;
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
        CheckValidVirtualPath(relative);

        if (IsRooted(relative))
        {
            path = relative;
        }
        else
        {
            // If the path is exactly "~", just return the app root path
            if (relative.Length == 1 && relative[0] == appRelativeCharacter)
                return appPath;

            // If the relative path starts with "~/" or "~\", treat it as app root
            // relative (ASURT 68628)
            if (IsAppRelative(relative))
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
                    if (list.Count == 1 && IsAppRelative(path))
                    {
                        Debug.Assert(sb.Length == 1);
                        return ReduceVirtualPath(MakeVirtualPathAppAbsolute(path));
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

    #endregion

    /// <summary>Returns the directory portion of a virtual path.</summary>
    /// <returns>The directory referenced in the virtual path. </returns>
    /// <param name="virtualPath">The virtual path.</param>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="virtualPath" /> is not rooted. - or -<paramref name="virtualPath" /> is null or an empty string.</exception>
    public static string? GetDirectory(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath))
            throw new ArgumentNullException(nameof(virtualPath), Empty_path_has_no_directory);

        if (virtualPath[0] != '/' && virtualPath[0] != appRelativeCharacter)
            throw new ArgumentException(string.Format(Path_must_be_rooted, virtualPath), nameof(virtualPath));

        if ((virtualPath[0] == appRelativeCharacter && virtualPath.Length==1) || virtualPath == appRelativeCharacterString) return "/";
        if (virtualPath.Length == 1) return null;

        var slashIndex = virtualPath.LastIndexOf('/');

        // This could happen if the input looks like "~abc"
        if (slashIndex < 0)
            throw new ArgumentException(string.Format(Path_must_be_rooted, virtualPath), nameof(virtualPath));

        return virtualPath[..(slashIndex + 1)];
    }

    /// <summary>Retrieves the extension of the file that is referenced in the virtual path.</summary>
    /// <returns>The file name extension string literal, including the period (.), null, or an empty string ("").</returns>
    /// <param name="virtualPath">The virtual path.</param>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="virtualPath" /> contains one or more characters that are not valid, as defined in <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
    public static string? GetExtension(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath)) throw new ArgumentNullException(nameof(virtualPath));

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

    /// <summary>Retrieves the file name of the file that is referenced in the virtual path.</summary>
    /// <returns>The file name literal after the last directory character in <paramref name="virtualPath" />; otherwise, the last directory name, if the last character of <paramref name="virtualPath" /> is a directory or volume separator character.</returns>
    /// <param name="virtualPath">The virtual path. </param>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="virtualPath" /> contains one or more characters that are not valid, as defined in <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
    public static string? GetFileName(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath)) throw new ArgumentNullException(nameof(virtualPath));
        if (!IsAppRelative(virtualPath) && !IsRooted(virtualPath)) throw new ArgumentException($"The relative virtual path '{virtualPath}' is not allowed here.", nameof(virtualPath));
        var length = virtualPath.Length;
        for (var i = length; --i >= 0;)
        {
            var ch = virtualPath[i];
            if (ch == '/')
                return virtualPath.Substring(i + 1, length - i - 1);

        }
        return virtualPath;
    }

    /// <summary>Returns a Boolean value indicating whether the specified virtual path is absolute; that is, it starts with a literal slash mark (/).</summary>
    /// <returns>true if <paramref name="virtualPath" /> is an absolute path and is not null or an empty string (""); otherwise, false.</returns>
    /// <param name="virtualPath">The virtual path to check. </param>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="virtualPath" /> is null.</exception>
    public static bool IsAbsolute(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath)) throw new ArgumentNullException(nameof(virtualPath));
        return IsRooted(virtualPath);
    }

    /// <summary>Returns a Boolean value indicating whether the specified virtual path is relative to the application.</summary>
    /// <returns>true if <paramref name="virtualPath" /> is relative to the application; otherwise, false.</returns>
    /// <param name="virtualPath">The virtual path to check. </param>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="virtualPath" /> is null.</exception>
    public static bool IsAppRelative(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath)) throw new ArgumentNullException(nameof(virtualPath));

        var len = virtualPath.Length;

        // Empty string case
        if (len == 0) return false;

        // It must start with ~
        if (virtualPath[0] != appRelativeCharacter) return false;

        // Single character case: "~"
        if (len == 1) return true;

        // If it's longer, checks if it starts with "~/" or "~\"
        return virtualPath[1] == '\\' || virtualPath[1] == '/';
    }

    // We use file: protocol instead of http:, so that Uri.MakeRelative behaves
    // in a case insensitive way (VSWhidbey 80078)
    private const string dummyProtocolAndServer = "file://foo";
    private static readonly char[] s_slashChars = new char[] { '\\', '/' };

    /// <summary>Returns the relative virtual path from one virtual path containing the root operator (the tilde [~]) to another.</summary>
    /// <returns>The relative virtual path from <paramref name="fromPath" /> to <paramref name="toPath" />.</returns>
    /// <param name="fromPath">The starting virtual path to return the relative virtual path from.</param>
    /// <param name="toPath">The ending virtual path to return the relative virtual path to.</param>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="fromPath" /> is not rooted.- or -<paramref name="toPath" /> is not rooted.</exception>
    public static string MakeRelative(string fromPath, string toPath)
    {
        // If either path is app relative (~/...), make it absolute, since the Uri
        // class wouldn't know how to deal with it.
        fromPath = MakeVirtualPathAppAbsolute(fromPath);
        toPath = MakeVirtualPathAppAbsolute(toPath);

        // Make sure both virtual paths are rooted
        if (!IsRooted(fromPath))
            throw new ArgumentException(string.Format(Path_must_be_rooted, fromPath));
        if (!IsRooted(toPath))
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
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="virtualPath" /> is not rooted. </exception>
    /// <exception cref="T:System.Web.HttpException">A leading double period (..) is used to exit above the top directory.</exception>
    public static string ToAbsolute(string virtualPath)
    {
        if (IsRooted(virtualPath)) return virtualPath;
        if (IsAppRelative(virtualPath)) return ReduceVirtualPath(MakeVirtualPathAppAbsolute(virtualPath));
        throw new ArgumentException($"The relative virtual path '{virtualPath}' is not allowed here.", nameof(virtualPath));
    }

    /// <summary>Converts a virtual path to an application absolute path using the specified application path.</summary>
    /// <returns>The absolute virtual path representation of <paramref name="virtualPath" />.</returns>
    /// <param name="virtualPath">The virtual path to convert to an application-relative path.</param>
    /// <param name="applicationPath">The application path to use to convert <paramref name="virtualPath" /> to a relative path.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="applicationPath" /> is not rooted.</exception>
    /// <exception cref="T:System.Web.HttpException">A leading double period (..) is used in the application path to exit above the top directory.</exception>
    public static string ToAbsolute(string virtualPath, string applicationPath)
    {
        if (string.IsNullOrEmpty(applicationPath)) throw new ArgumentNullException(nameof(applicationPath));
        if (!IsRooted(applicationPath)) throw new ArgumentException($"The relative virtual path '{virtualPath}' is not allowed here.", nameof(applicationPath));
        if (IsRooted(virtualPath)) return virtualPath;
        var appPath = AppendTrailingSlash(applicationPath);
        if (IsAppRelative(virtualPath)) return ReduceVirtualPath(MakeVirtualPathAppAbsolute(virtualPath, appPath));
        throw new ArgumentException($"The relative virtual path '{virtualPath}' is not allowed here.", nameof(virtualPath));
    }

    /// <summary>Converts a virtual path to an application-relative path using the application virtual path that is in the <see cref="P:System.Web.HttpRuntime.AppDomainAppVirtualPath" /> property. </summary>
    /// <returns>The application-relative path representation of <paramref name="virtualPath" />.</returns>
    /// <param name="virtualPath">The virtual path to convert to an application-relative path. </param>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="virtualPath" /> is null. </exception>
    public static string ToAppRelative(string virtualPath) => ToAppRelative(virtualPath, HttpRuntime.AppDomainAppVirtualPath);

    /// <summary>Converts a virtual path to an application-relative path using a specified application path.</summary>
    /// <returns>The application-relative path representation of <paramref name="virtualPath" />.</returns>
    /// <param name="virtualPath">The virtual path to convert to an application-relative path. </param>
    /// <param name="applicationPath">The application path to use to convert <paramref name="virtualPath" /> to a relative path. </param>
    public static string ToAppRelative(string virtualPath, string applicationPath)
    {
        var appPath = AppendTrailingSlash(applicationPath);
        if (virtualPath == null)
            throw new ArgumentNullException(nameof(virtualPath));

        var appPathLength = appPath.Length;
        var virtualPathLength = virtualPath.Length;

        // If virtualPath is the same as the app path, but without the ending slash,
        // treat it as if it were truly the app path (VSWhidbey 495949)
        if (virtualPathLength == appPathLength - 1)
        {
            if (StringStartsWithIgnoreCase(appPath, virtualPath))
                return appRelativeCharacterString;
        }

        if (!VirtualPathStartsWithVirtualPath(virtualPath, appPath))
            return virtualPath;

        // If they are the same, just return "~/"
        if (virtualPathLength == appPathLength)
            return appRelativeCharacterString;

        // Special case for apps rooted at the root:
        if (appPathLength == 1)
            return appRelativeCharacter + virtualPath;

        return appRelativeCharacter + virtualPath[(appPathLength - 1)..];
    }

    private static string MakeVirtualPathAppAbsolute(string virtualPath)
    {
        return MakeVirtualPathAppAbsolute(virtualPath, HttpRuntime.AppDomainAppVirtualPath);
    }

    // If a virtual path is app relative (i.e. starts with ~/), change it to
    // start with the actuall app path.
    // E.g. ~/Sub/foo.aspx --> /MyApp/Sub/foo.aspx
    private static string MakeVirtualPathAppAbsolute(string virtualPath, string applicationPath)
    {
        // If the path is exactly "~", just return the app root path
        if (virtualPath.Length == 1 && virtualPath[0] == appRelativeCharacter)
            return applicationPath;

        // If the virtual path starts with "~/" or "~\", replace with the app path
        // relative (ASURT 68628)
        if (virtualPath.Length >= 2 && virtualPath[0] == appRelativeCharacter &&
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
        if (!IsRooted(virtualPath))
            throw new ArgumentOutOfRangeException(nameof(virtualPath));

        // Return it unchanged
        return virtualPath;
    }

    private static bool VirtualPathStartsWithVirtualPath(string virtualPath1, string virtualPath2)
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
        if (!StringStartsWithIgnoreCase(virtualPath1, virtualPath2))
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

    /*
	 * Determines if the first string starts with the second string, ignoring case.
	 * Fast, non-culture aware.  
	 */
    private static bool StringStartsWithIgnoreCase(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return false;
        if (s2.Length > s1.Length) return false;
        return s1.StartsWith(s2, StringComparison.OrdinalIgnoreCase);
    }
}
