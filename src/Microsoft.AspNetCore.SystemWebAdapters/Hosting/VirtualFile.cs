// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
