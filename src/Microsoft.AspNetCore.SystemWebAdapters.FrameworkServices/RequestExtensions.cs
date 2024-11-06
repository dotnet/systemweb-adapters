// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static class RequestExtensions
{
    /// <summary>
    /// Gets the request stream. If it has already been retrieved, it will be in whatever <see cref="HttpRequest.ReadEntityBodyMode"/>
    /// it was already accessed. Otherwise, it will get a bufferless version via <see cref="HttpRequest.GetBufferlessInputStream()"/>.
    /// </summary>
    /// <param name="request">The request</param>
    /// <returns></returns>
    public static Stream GetInputStream(this HttpRequestBase request)
    {
        // If a call has been made to InputStream, we must use that. It is seekable, so we need to rewind it to the beginning
        if (request.ReadEntityBodyMode == ReadEntityBodyMode.Classic)
        {
            var stream = request.InputStream;
            stream.Position = 0;
            return stream;
        }

        // If a call has been made to GetBufferedInputStream, we must use that. It is seekable, so we need to rewind it to the beginning
        if (request.ReadEntityBodyMode == ReadEntityBodyMode.Buffered)
        {
            var stream = request.GetBufferedInputStream();
            stream.Position = 0;
            return stream;
        }

        // Otherwise, let's attempt to get a bufferless version since we don't need it buffered
        return request.GetBufferlessInputStream();
    }
}
