// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NET6_0_OR_GREATER

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal interface IHttpRequestPathFeature
{
    string Path { get; }

    string RawUrl { get; }

    string PathInfo { get; }

    string FilePath { get; }
}

#endif
