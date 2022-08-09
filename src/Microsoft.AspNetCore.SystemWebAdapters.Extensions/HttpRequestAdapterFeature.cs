// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// This feature implements the <see cref="IHttpRequestAdapterFeature"/> to expose functionality for the adapters. As part of that,
/// it overrides the following features as well:
/// 
/// <list>
///   <item>
///     <see cref="IHttpResponseBodyFeature"/>: Provide ability to turn off writing to the stream, while also supporting the ability to clear and suppress output
///   </item>
/// </list> 
/// </summary>
internal class HttpRequestAdapterFeature : Stream, IHttpResponseBodyFeature, IHttpRequestAdapterFeature
{
    private enum StreamState
    {
        NotStarted,
        Buffering,
        NotBuffering,
        Complete,
    }

    private readonly IHttpResponseBodyFeature _responseBodyFeature;
    private readonly BufferResponseStreamAttribute _metadata;

    private FileBufferingWriteStream? _bufferedStream;
    private PipeWriter? _pipeWriter;
    private bool _suppressContent;
    private StreamState _state; 

    public HttpRequestAdapterFeature(IHttpResponseBodyFeature httpResponseBody, BufferResponseStreamAttribute metadata)
    {
        _responseBodyFeature = httpResponseBody;
        _metadata = metadata;
        _state = StreamState.NotStarted;
    }

    Task IHttpResponseBodyFeature.CompleteAsync() => CompleteAsync();

    void IHttpResponseBodyFeature.DisableBuffering()
    {
        if (_state == StreamState.NotStarted)
        {
            _state = StreamState.NotBuffering;
            _responseBodyFeature.DisableBuffering();
            _pipeWriter = _responseBodyFeature.Writer;
        }
    }

    Task IHttpResponseBodyFeature.StartAsync(CancellationToken cancellationToken)
    {
        if (_state == StreamState.NotStarted)
        {
            _state = StreamState.Buffering;
        }

        return _responseBodyFeature.StartAsync(cancellationToken);
    }

    Stream IHttpResponseBodyFeature.Stream => this;

    PipeWriter IHttpResponseBodyFeature.Writer => _pipeWriter ??= PipeWriter.Create(this, new StreamPipeWriterOptions(leaveOpen: true));

    bool IHttpRequestAdapterFeature.SuppressContent
    {
        get => _suppressContent;
        set => _suppressContent = value;
    }

    Task IHttpRequestAdapterFeature.EndAsync() => CompleteAsync();

    bool IHttpRequestAdapterFeature.IsEnded => _state == StreamState.Complete;

    void IHttpRequestAdapterFeature.ClearContent()
    {
        if (_bufferedStream is not null)
        {
            _bufferedStream.Dispose();
            _bufferedStream = null;
        }
    }

    private Stream CurrentStream
    {
        get
        {
            if (_state == StreamState.NotBuffering)
            {
                return _responseBodyFeature.Stream;
            }
            else if (_state == StreamState.Complete)
            {
                return Null;
            }
            else
            {
                _state = StreamState.Buffering;
                return _bufferedStream ??= new FileBufferingWriteStream(_metadata.MemoryThreshold, _metadata.BufferLimit);
            }
        }
    }

    public override async ValueTask DisposeAsync()
    {
        if (_bufferedStream is not null)
        {
            await _bufferedStream.DisposeAsync();
        }

        await base.DisposeAsync();
    }

    public async ValueTask FlushBufferedStreamAsync()
    {
        if (_state is StreamState.Buffering && _bufferedStream is not null && !_suppressContent)
        {
            await _bufferedStream.DrainBufferAsync(_responseBodyFeature.Stream);
        }
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => CurrentStream.Length;

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }


    private async Task CompleteAsync()
    {
        await FlushBufferedStreamAsync();
        await _responseBodyFeature.CompleteAsync();
        _state = StreamState.Complete;
    }

    public override void Flush() => CurrentStream.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken) => CurrentStream.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken = default)
        => SendFileFallback.SendFileAsync(CurrentStream, path, offset, count, cancellationToken);

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => CurrentStream.Write(buffer, offset, count);

    public override void Write(ReadOnlySpan<byte> buffer) => CurrentStream.Write(buffer);

    public override void WriteByte(byte value) => CurrentStream.WriteByte(value);

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        => CurrentStream.WriteAsync(buffer, cancellationToken);

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => CurrentStream.WriteAsync(buffer, offset, count, cancellationToken);
}
