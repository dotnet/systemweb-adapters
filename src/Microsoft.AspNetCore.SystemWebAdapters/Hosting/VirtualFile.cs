// MIT License.


// MIT License.

using System.IO;

namespace System.Web.Hosting;

public abstract class VirtualFile : VirtualFileBase
{
    protected VirtualFile(string virtualPath)
        : base(virtualPath)
    {
    }

    public override bool IsDirectory => false;

    public abstract Stream Open();
}
