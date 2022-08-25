// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class HttpRequestAdapterFeature : IHttpRequestAdapterFeature, IHttpRequestFeature, IDisposable
{
    private readonly int _bufferThreshold;
    private readonly long? _bufferLimit;
    private readonly IHttpRequestFeature _other;

    private Stream? _bufferedStream;

    public HttpRequestAdapterFeature(IHttpRequestFeature other, int bufferThreshold, long? bufferLimit)
    {
        _bufferThreshold = bufferThreshold;
        _bufferLimit = bufferLimit;
        _other = other;
    }

    public ReadEntityBodyMode Mode { get; private set; }

    public Stream GetBufferedInputStream()
    {
        if (Mode is ReadEntityBodyMode.Buffered)
        {
            Debug.Assert(_bufferedStream is not null);
            return _bufferedStream;
        }

        if (Mode is ReadEntityBodyMode.None)
        {
            Mode = ReadEntityBodyMode.Buffered;

            return _bufferedStream = new FileBufferingReadStream(_other.Body, _bufferThreshold, _bufferLimit, AspNetCoreTempDirectory.TempDirectoryFactory);
        }

        throw new InvalidOperationException("GetBufferlessInputStream cannot be called after other stream access");
    }

    Stream IHttpRequestAdapterFeature.GetBufferlessInputStream()
    {
        if (Mode is ReadEntityBodyMode.Bufferless or ReadEntityBodyMode.None)
        {
            Mode = ReadEntityBodyMode.Bufferless;
            return GetBody();
        }

        throw new InvalidOperationException("GetBufferlessInputStream cannot be called after other stream access");
    }

    Stream IHttpRequestAdapterFeature.InputStream
    {
        get
        {
            if (Mode is ReadEntityBodyMode.Classic && _bufferedStream is not null)
            {
                return _bufferedStream;
            }

            throw new InvalidOperationException("InputStream must be prebuffered");
        }
    }

    async Task<Stream> IHttpRequestAdapterFeature.GetInputStreamAsync(CancellationToken token)
    {
        await BufferInputStreamAsync(token);
        return GetBody();
    }

    public async Task BufferInputStreamAsync(CancellationToken token)
    {
        if (Mode is ReadEntityBodyMode.Classic)
        {
            return;
        }

        if (Mode is not ReadEntityBodyMode.None)
        {
            throw new InvalidOperationException("InputStream cannot be called after other stream access");
        }

        var stream = GetBufferedInputStream();
        await stream.DrainAsync(token);
        stream.Position = 0;

        Mode = ReadEntityBodyMode.Classic;
    }

    public void Dispose() => _bufferedStream?.Dispose();

    string IHttpRequestFeature.Protocol
    {
        get => _other.Protocol;
        set => _other.Protocol = value;
    }

    string IHttpRequestFeature.Scheme
    {
        get => _other.Scheme;
        set => _other.Scheme = value;
    }

    string IHttpRequestFeature.Method
    {
        get => _other.Method;
        set => _other.Method = value;
    }

    string IHttpRequestFeature.PathBase
    {
        get => _other.PathBase;
        set => _other.PathBase = value;
    }

    string IHttpRequestFeature.Path
    {
        get => _other.Path;
        set => _other.Path = value;
    }

    string IHttpRequestFeature.QueryString
    {
        get => _other.QueryString;
        set => _other.QueryString = value;
    }

    string IHttpRequestFeature.RawTarget
    {
        get => _other.RawTarget;
        set => _other.RawTarget = value;
    }

    IHeaderDictionary IHttpRequestFeature.Headers
    {
        get => _other.Headers;
        set => _other.Headers = value;
    }

    Stream IHttpRequestFeature.Body
    {
        get
        {
            var body = GetBody();

            if (Mode is ReadEntityBodyMode.None)
            {
                Mode = body.CanSeek ? ReadEntityBodyMode.Buffered : ReadEntityBodyMode.Bufferless;
            }

            return body;
        }
        set => _other.Body = value;
    }

    private Stream GetBody() => _bufferedStream ?? _other.Body;

    internal static class AspNetCoreTempDirectory
    {
        private static string? _tempDirectory;

        public static string TempDirectory
        {
            get
            {
                if (_tempDirectory == null)
                {
                    // Look for folders in the following order.
                    var temp = Environment.GetEnvironmentVariable("ASPNETCORE_TEMP") ?? // ASPNETCORE_TEMP - User set temporary location.
                               Path.GetTempPath();                                      // Fall back.

                    if (!Directory.Exists(temp))
                    {
                        throw new DirectoryNotFoundException(temp);
                    }

                    _tempDirectory = temp;
                }

                return _tempDirectory;
            }
        }

        public static Func<string> TempDirectoryFactory => () => TempDirectory;
    }
}
