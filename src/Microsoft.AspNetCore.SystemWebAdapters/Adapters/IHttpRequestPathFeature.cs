// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

internal interface IHttpRequestPathFeature
{
    string Path { get; }

    string PathInfo { get; }

    string FilePath { get; }

    string RawUrl { get; }

    void Rewrite(string filePath, string pathInfo, string? queryString, bool setClientFilePath);
}

#endif
