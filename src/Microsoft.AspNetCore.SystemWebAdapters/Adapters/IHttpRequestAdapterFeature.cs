// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal interface IHttpRequestAdapterFeature
{
    ReadEntityBodyMode Mode { get; }

    Task<Stream> GetInputStreamAsync(CancellationToken token);

    Stream InputStream { get; }

    Stream GetBufferedInputStream();

    Stream GetBufferlessInputStream();
}

#endif
