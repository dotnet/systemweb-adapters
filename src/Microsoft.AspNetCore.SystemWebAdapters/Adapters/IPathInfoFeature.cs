// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NET6_0_OR_GREATER
namespace Microsoft.AspNetCore.SystemWebAdapters;

internal interface IPathInfoFeature
{
    string PathInfo { get; }

    string FileInfo { get; }
}
#endif
