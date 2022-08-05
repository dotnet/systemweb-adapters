// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;

#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.Http;
#endif

namespace System.Web;

public static class HttpResponseAsyncExtensions
{
    public static Task WriteFileAsync(this HttpResponse response, string filename, CancellationToken token)
#if NET6_0_OR_GREATER
        => response.TransmitFileAsync(filename, token);
#else
    {
        response.WriteFile(filename);
        return Task.CompletedTask;
    }
#endif

    public static Task TransmitFileAsync(this HttpResponse response, string filename, long offset, long length, CancellationToken token)
#if NET6_0_OR_GREATER
        => response.UnwrapAdapter().SendFileAsync(filename, offset, length >= 0 ? length : null, token);
#else
    {
        response.TransmitFile(filename, offset, length);
        return Task.CompletedTask;
    }
#endif

    public static Task TransmitFileAsync(this HttpResponse response, string filename, CancellationToken token)
        => response.TransmitFileAsync(filename, 0, -1, token);
}
