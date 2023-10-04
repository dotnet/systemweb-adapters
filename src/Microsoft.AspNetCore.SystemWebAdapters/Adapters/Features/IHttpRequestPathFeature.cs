// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

using System.Diagnostics.CodeAnalysis;
using System.Web;

/// <summary>
/// Represents the path-related members on <see cref="HttpRequest"/>.
/// </summary>
#if NET8_0_OR_GREATER
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public
#else
internal
#endif
interface IHttpRequestPathFeature
{
    string Path { get; }

    string PathInfo { get; }

    string FilePath { get; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = Constants.ApiFromAspNet)]
    string RawUrl { get; }

    string? PhysicalPath { get; }

    string CurrentExecutionFilePath { get; }

    void Rewrite(string filePath, string pathInfo, string? queryString, bool setClientFilePath);
}

#endif
