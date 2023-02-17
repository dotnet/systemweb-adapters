// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal interface IHttpRequestInputStreamFeature
{
    /// <summary>
    /// Gets the <see cref="ReadEntityBodyMode"/> of the request.
    /// </summary>
    ReadEntityBodyMode Mode { get; }

    /// <summary>
    /// Buffers the input stream and drains it all.
    /// </summary>
    ValueTask BufferInputStreamAsync(CancellationToken token);

    /// <summary>
    /// Gets or sets the buffer threshold when buffering is done.
    /// </summary>
    int BufferThreshold { get; set; }

    /// <summary>
    /// Gets or sets the buffer limit for when buffering is done.
    /// </summary>
    long? BufferLimit { get; set; }

    /// <summary>
    /// Gets a fully buffered and drained InputStream. Requires a call to <see cref="BufferInputStreamAsync(CancellationToken)"/> before a call.
    /// </summary>
    Stream InputStream { get; }

    /// <summary>
    /// Gets a buffered input stream that can be reread, but has not been drained.
    /// </summary>
    Stream GetBufferedInputStream();

    /// <summary>
    /// Gets a non-buffered stream; similar behavior to <see cref="HttpRequestCore.Body"/>.
    /// </summary>
    Stream GetBufferlessInputStream();
}

#endif
