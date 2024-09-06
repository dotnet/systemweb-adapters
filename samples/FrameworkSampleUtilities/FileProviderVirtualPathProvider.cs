// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// An implementation of <see cref="VirtualPathProvider"/> that accepts a <see cref="IFileProvider"/>
/// and checks there before passing it to the existing <see cref="VirtualPathProvider"/>
/// </summary>
internal class FileProviderVirtualPathProvider : VirtualPathProvider
{
    private readonly IFileProvider _provider;

    public FileProviderVirtualPathProvider(IFileProvider provider)
    {
        _provider = provider;
    }

    private static string NormalizePath(string url)
    {
        if (!url.StartsWith("~", StringComparison.Ordinal))
        {
            var urlSb = new StringBuilder(url);
            Normalize(urlSb);
            return urlSb.ToString();
        }

        var vdir = HttpRuntime.AppDomainAppVirtualPath;

        // start from after the '~'
        var sb = new StringBuilder(url, 1, url.Length - 1, url.Length + vdir.Length);

        sb.Insert(0, vdir);

        Normalize(sb);

        return sb.ToString();

        static void Normalize(StringBuilder sb)
        {
            if (sb.Length > 0 && sb[0] == '/')
            {
                sb.Remove(0, 1);
            }

            sb.Replace("//", "");
            sb.Replace("/", "\\");
        }
    }


    public override VirtualFile GetFile(string virtualPath)
    {
        if (_provider.GetFileInfo(NormalizePath(virtualPath)) is { Exists: true } fileInfo)
        {
            return new File(fileInfo, virtualPath);
        }

        return base.GetFile(virtualPath);
    }

    public override VirtualDirectory GetDirectory(string virtualDir)
        => new Dir(_provider, base.GetDirectory(virtualDir), virtualDir);

    public override bool DirectoryExists(string virtualDir)
        => _provider.GetDirectoryContents(NormalizePath(virtualDir)) is { Exists: true } || base.DirectoryExists(virtualDir);

    public override bool FileExists(string virtualPath)
        => _provider.GetFileInfo(NormalizePath(virtualPath)) is { IsDirectory: false, Exists: true } || base.FileExists(virtualPath);

    public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
    {
        return base.GetCacheDependency(virtualPath, FilterDependencies(virtualPathDependencies.Cast<string>()), utcStart);
    }

    private IEnumerable<string> FilterDependencies(IEnumerable<string> dependencies)
    {
        foreach (var d in dependencies)
        {
            if (base.FileExists(d))
            {
                yield return d;
            }
        }
    }

    private sealed class File : VirtualFile
    {
        private readonly IFileInfo _file;

        public File(IFileInfo file, string virtualPath)
            : base(virtualPath)
        {
            _file = file;
        }

        public override Stream Open() => _file.CreateReadStream();
    }

    private sealed class Dir : VirtualDirectory
    {
        private readonly IFileProvider _files;
        private readonly VirtualDirectory? _other;

        public Dir(IFileProvider files, VirtualDirectory? other, string virtualPath)
            : base(virtualPath)
        {
            _files = files;
            _other = other;
        }

        public override IEnumerable Directories => GetAll(false, true, _other?.Directories);

        public override IEnumerable Files => GetAll(true, false, _other?.Files);

        public override IEnumerable Children => GetAll(true, true, _other?.Children);

        private IEnumerable GetAll(bool returnFiles, bool returnDirectories, IEnumerable? other)
        {
            foreach (var item in _files.GetDirectoryContents(VirtualPath))
            {
                if (returnFiles && !item.IsDirectory)
                {
                    yield return new File(item, Path.Combine(VirtualPath, item.Name));
                }
                else if (returnDirectories && item.IsDirectory)
                {
                    yield return new Dir(_files, null, VirtualPath + "/" + item.Name);
                }
            }

            if (other is { })
            {
                foreach (var item in other)
                {
                    yield return item;
                }
            }
        }
    }
}
