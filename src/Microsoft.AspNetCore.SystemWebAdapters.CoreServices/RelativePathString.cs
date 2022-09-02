// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal readonly struct RelativePathString
{
    public RelativePathString(PathString path)
    {
        Path = path;
        Relative = "." + path;
    }

    public PathString Path { get; }

    /// <summary>
    /// Use when you want the path to be relative to whatever URI you may combine it with
    /// </summary>
    public string Relative { get; }
}
