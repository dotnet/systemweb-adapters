// MIT License.

using System.Collections;
using System.Web.Caching;
using System.Web.Util;

namespace System.Web.Hosting;

public abstract class VirtualPathProvider
{
    internal virtual void Initialize(VirtualPathProvider? previous)
    {
        Previous = previous;
        Initialize();
    }

    protected virtual void Initialize()
    {
    }

    protected internal VirtualPathProvider? Previous { get; private set; }

    public virtual string? GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        => Previous?.GetFileHash(virtualPath, virtualPathDependencies);

    public virtual CacheDependency? GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        => Previous?.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);

    public virtual bool FileExists(string virtualPath) => Previous != null && Previous.FileExists(virtualPath);

    public virtual bool DirectoryExists(string virtualDir) => Previous != null && Previous.DirectoryExists(virtualDir);

    public virtual VirtualFile? GetFile(string virtualPath) => Previous?.GetFile(virtualPath);

    internal VirtualFile? GetFileWithCheck(string virtualPath)
    {
        var virtualFile = GetFile(virtualPath);

        if (virtualFile == null)
        {
            return null;
        }

        // Make sure the VirtualFile's path is the same as what was passed to GetFile
        if (!string.Equals(virtualPath, virtualFile.VirtualPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new HttpException($"Bad virtual path {virtualFile} in VirtuaPathBase");
        }

        return virtualFile;
    }

    public virtual VirtualDirectory? GetDirectory(string virtualDir) => Previous?.GetDirectory(virtualDir);

    public virtual string CombineVirtualPaths(string basePath, string relativePath)
    {
        if (string.IsNullOrEmpty(basePath))
        {
            throw new ArgumentException($"'{nameof(basePath)}' cannot be null or empty.", nameof(basePath));
        }

        var baseDir = UrlPath.GetDirectory(basePath);

        // By default, just combine them normally
        return VirtualPathUtility.Combine(baseDir, relativePath);
    }
}
