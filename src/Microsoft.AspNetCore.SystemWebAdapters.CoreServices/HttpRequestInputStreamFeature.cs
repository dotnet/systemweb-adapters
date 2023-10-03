// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class HttpRequestInputStreamFeature : IHttpRequestInputStreamFeature, IHttpRequestPathFeature, IHttpRequestFeature, IRequestBodyPipeFeature, IDisposable
{
    private readonly IHttpRequestFeature _other;

    private PipeReader? _pipeReader;
    private Stream? _bufferedStream;

    private string? _pathInfo;
    private string? _filePath;

    public HttpRequestInputStreamFeature(IHttpRequestFeature other)
    {
        BufferThreshold = PreBufferRequestStreamAttribute.DefaultBufferThreshold;
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

            return _bufferedStream = new FileBufferingReadStream(_other.Body, BufferThreshold, BufferLimit, AspNetCoreTempDirectory.TempDirectoryFactory);
        }

        throw new InvalidOperationException("GetBufferlessInputStream cannot be called after other stream access");
    }

    public int BufferThreshold { get; set; }

    public long? BufferLimit { get; set; }

    Stream IHttpRequestInputStreamFeature.GetBufferlessInputStream()
    {
        if (Mode is ReadEntityBodyMode.Bufferless or ReadEntityBodyMode.None)
        {
            Mode = ReadEntityBodyMode.Bufferless;
            return GetBody();
        }

        throw new InvalidOperationException("GetBufferlessInputStream cannot be called after other stream access");
    }

    Stream IHttpRequestInputStreamFeature.InputStream
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

    async ValueTask IHttpRequestInputStreamFeature.BufferInputStreamAsync(CancellationToken token)
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

    public string Path
    {
        get => _other.Path;
        set
        {
            _filePath = null;
            _pathInfo = null;
            _other.Path = value;
        }
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

    public Stream Body
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
        set
        {
            _other.Body = value;
            Reset();
        }
    }

    PipeReader IRequestBodyPipeFeature.Reader => _pipeReader ??= PipeReader.Create(Body, new StreamPipeReaderOptions(leaveOpen: true));

    private void Reset()
    {
        Mode = ReadEntityBodyMode.None;

        if (_bufferedStream is not null)
        {
            _bufferedStream.Dispose();
            _bufferedStream = null;
        }
    }

    private Stream GetBody() => _bufferedStream ?? _other.Body;

    void IHttpRequestPathFeature.Rewrite(string filePath, string pathInfo, string? queryString, bool setClientFilePath)
    {
        _other.QueryString = queryString ?? string.Empty;

        if (string.IsNullOrEmpty(pathInfo))
        {
            Path = filePath;
        }
        else if (pathInfo.StartsWith('/'))
        {
            Path = $"{filePath}{pathInfo}";
        }
        else
        {
            Path = $"{filePath}/{pathInfo}";
        }

        // This must be set after setting Path as it will reset the PathInfo and FilePath instances
        _pathInfo = pathInfo;
        _filePath = filePath;
    }

    string IHttpRequestPathFeature.PathInfo => _pathInfo ?? string.Empty;

    string IHttpRequestPathFeature.FilePath => _filePath ?? Path;

    string IHttpRequestPathFeature.RawUrl => _other.RawTarget;

    string IHttpRequestPathFeature.CurrentExecutionFilePath => _filePath ?? Path;

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
                               System.IO.Path.GetTempPath();                                      // Fall back.

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
