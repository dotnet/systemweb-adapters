// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web.Util;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Options;

namespace System.Web;

internal sealed class VirtualPathUtilityImpl
{
    private const string Empty_path_has_no_directory = "Empty path has no directory.";

    private readonly IOptions<HostingEnvironmentOptions> _options;
    private readonly UrlPath _urlPath;

    public VirtualPathUtilityImpl(IOptions<HostingEnvironmentOptions> options)
    {
        _options = options;
        _urlPath = new UrlPath(options);
    }

    [return: NotNullIfNotNull(nameof(virtualPath))]
    public static string? AppendTrailingSlash(string? virtualPath)
    {
        if (virtualPath == null)
        {
            return null;
        }

        var l = virtualPath.Length;
        if (l == 0)
        {
            return virtualPath;
        }

        if (virtualPath[l - 1] != '/')
        {
            virtualPath += '/';
        }

        return virtualPath;
    }

    public string Combine(string basePath, string relativePath)
        => _urlPath.Combine(_options.Value.AppDomainAppVirtualPath, basePath, relativePath);

    public static string? GetDirectory(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath))
        {
            throw new ArgumentNullException(nameof(virtualPath), Empty_path_has_no_directory);
        }

        if (virtualPath[0] != '/' && virtualPath[0] != UrlPath.AppRelativeCharacter)
        {
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, UrlPath.Path_must_be_rooted, virtualPath), nameof(virtualPath));
        }

        if ((virtualPath[0] == UrlPath.AppRelativeCharacter && virtualPath.Length == 1) || virtualPath == UrlPath.AppRelativeCharacterString)
        {
            return "/";
        }

        if (virtualPath.Length == 1)
        {
            return null;
        }

        var slashIndex = virtualPath.LastIndexOf('/');

        // This could happen if the input looks like "~abc"
        if (slashIndex < 0)
        {
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, UrlPath.Path_must_be_rooted, virtualPath), nameof(virtualPath));
        }

        return virtualPath[..(slashIndex + 1)];
    }

    public static string? GetExtension(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath))
        {
            throw new ArgumentNullException(nameof(virtualPath));
        }

        return UrlPath.GetExtension(virtualPath);
    }

    public static string? GetFileName(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath))
        {
            throw new ArgumentNullException(nameof(virtualPath));
        }

        if (!IsAppRelative(virtualPath) && !UrlPath.IsRooted(virtualPath))
        {
            throw new ArgumentException($"The relative virtual path '{virtualPath}' is not allowed here.", nameof(virtualPath));
        }

        return UrlPath.GetFileName(virtualPath);
    }

    public static bool IsAbsolute(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath))
        {
            throw new ArgumentNullException(nameof(virtualPath));
        }

        return UrlPath.IsRooted(virtualPath);
    }

    public static bool IsAppRelative(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath))
        {
            throw new ArgumentNullException(nameof(virtualPath));
        }

        return UrlPath.IsAppRelativePath(virtualPath);
    }

    public string MakeRelative(string fromPath, string toPath) => _urlPath.MakeRelative(fromPath, toPath);

    public static string? RemoveTrailingSlash(string virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath))
        {
            return null;
        }

        var l = virtualPath.Length;
        if (l <= 1 || virtualPath[l - 1] != '/')
        {
            return virtualPath;
        }

        return virtualPath[..(l - 1)];
    }

    public string ToAbsolute(string virtualPath)
    {
        if (UrlPath.IsRooted(virtualPath))
        {
            return virtualPath;
        }

        if (IsAppRelative(virtualPath))
        {
            return _urlPath.ReduceVirtualPath(_urlPath.MakeVirtualPathAppAbsolute(virtualPath));
        }

        throw new ArgumentException($"The relative virtual path '{virtualPath}' is not allowed here.", nameof(virtualPath));
    }

    public string ToAbsolute(string virtualPath, string applicationPath)
    {
        if (string.IsNullOrEmpty(applicationPath))
        {
            throw new ArgumentNullException(nameof(applicationPath));
        }

        if (!UrlPath.IsRooted(applicationPath))
        {
            throw new ArgumentException($"The relative virtual path '{virtualPath}' is not allowed here.", nameof(applicationPath));
        }

        if (UrlPath.IsRooted(virtualPath))
        {
            return virtualPath;
        }

        var appPath = AppendTrailingSlash(applicationPath);

        if (IsAppRelative(virtualPath))
        {
            return _urlPath.ReduceVirtualPath(UrlPath.MakeVirtualPathAppAbsolute(virtualPath, appPath));
        }

        throw new ArgumentException($"The relative virtual path '{virtualPath}' is not allowed here.", nameof(virtualPath));
    }

    public string ToAppRelative(string virtualPath) => ToAppRelative(virtualPath, _options.Value.AppDomainAppVirtualPath);

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
            if (StringUtil.StringStartsWithIgnoreCase(appPath, virtualPath))
            {
                return UrlPath.AppRelativeCharacterString;
            }
        }

        if (!UrlPath.VirtualPathStartsWithVirtualPath(virtualPath, appPath))
        {
            return virtualPath;
        }

        // If they are the same, just return "~/"
        if (virtualPathLength == appPathLength)
        {
            return UrlPath.AppRelativeCharacterString;
        }

        // Special case for apps rooted at the root:
        if (appPathLength == 1)
        {
            return UrlPath.AppRelativeCharacter + virtualPath;
        }

        return UrlPath.AppRelativeCharacter + virtualPath[(appPathLength - 1)..];
    }
}
