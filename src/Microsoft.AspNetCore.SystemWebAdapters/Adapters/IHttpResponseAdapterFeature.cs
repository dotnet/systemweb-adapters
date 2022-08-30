// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal interface IHttpResponseAdapterFeature
{
    bool IsEnded { get; }

    bool SuppressContent { get; set; }

    Task EndAsync();

    void ClearContent();
}

#endif
