// MIT License.

namespace System.Web.Hosting;

public abstract class VirtualFileBase
{
    private protected VirtualFileBase(string virtualPath)
    {
        VirtualPath = virtualPath;
    }

    public virtual string Name => VirtualPathUtility.GetFileName(VirtualPath);

    public string VirtualPath { get; }

    public abstract bool IsDirectory { get; }
}
