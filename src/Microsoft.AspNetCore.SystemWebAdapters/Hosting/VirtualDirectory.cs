// MIT License.

using System.Collections;

namespace System.Web.Hosting;

public abstract class VirtualDirectory : VirtualFileBase
{
    protected VirtualDirectory(string virtualPath)
        : base(VirtualPathUtility.AppendTrailingSlash(virtualPath))
    {
    }

    public override bool IsDirectory => true;

    /// <summary>
    /// Returns an object that enumerates all the children VirtualDirectory's of this directory.
    /// </summary>
    public abstract IEnumerable Directories { get; }

    /// <summary>
    ///Returns an object that enumerates all the children VirtualFile's of this directory.
    /// </summary>
    public abstract IEnumerable Files { get; }

    /// <summary>
    /// Returns an object that enumerates all the children VirtualDirectory's and VirtualFiles of this directory.
    /// </summary>
    public abstract IEnumerable Children { get; }
}
