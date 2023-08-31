// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class HttpResponseAdapterFeature : Stream, IHttpResponseBodyFeature, IHttpResponseBufferingFeature, IHttpResponseEndFeature, IHttpResponseContentFeature
{
    private enum StreamState
    {
        NotStarted,
        Buffering,
        NotBuffering,
        Complete,
    }

    private readonly IHttpResponseBodyFeature _responseBodyFeature;

    private FileBufferingWriteStream? _bufferedStream;
    private PipeWriter? _pipeWriter;
    private StreamState _state;
    private Func<FileBufferingWriteStream>? _factory;
    private bool _suppressContent;

    public HttpResponseAdapterFeature(IHttpResponseBodyFeature httpResponseBody)
    {
        _responseBodyFeature = httpResponseBody;
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

    void IHttpResponseBufferingFeature.EnableBuffering(int memoryThreshold, long? bufferLimit)
    {
        if (_state == StreamState.NotStarted)
        {
            Debug.Assert(_bufferedStream is null);

            _state = StreamState.Buffering;
            _factory = () => new FileBufferingWriteStream(memoryThreshold, bufferLimit);
        }
        else
        {
            throw new InvalidOperationException("Cannot enable buffering if writing has begun");
        }
    }

    Task IHttpResponseBodyFeature.StartAsync(CancellationToken cancellationToken)
    {
        if (_state == StreamState.NotStarted)
        {
            _state = StreamState.NotBuffering;
        }

        return _responseBodyFeature.StartAsync(cancellationToken);
    }

    bool IHttpResponseBufferingFeature.IsEnabled
    {
        get
        {
            return _state != StreamState.NotBuffering && _state != StreamState.NotStarted;
        }
    }

    private async ValueTask FlushInternalAsync()
    {
        if (_state is StreamState.Buffering && _bufferedStream is not null && !SuppressContent)
        {
            await _bufferedStream.DrainBufferAsync(_responseBodyFeature.Stream);
            await _bufferedStream.DisposeAsync();
            _bufferedStream = null;
        }
    }

    Stream IHttpResponseBodyFeature.Stream => this;

    PipeWriter IHttpResponseBodyFeature.Writer => _pipeWriter ??= PipeWriter.Create(this, new StreamPipeWriterOptions(leaveOpen: true));

    public bool SuppressContent
    {
        get => _suppressContent;
        set
        {
            if (value)
            {
                VerifyBuffering();
            }

            _suppressContent = value;
        }
    }

    Task IHttpResponseEndFeature.EndAsync() => CompleteAsync();

    bool IHttpResponseEndFeature.IsEnded => _state == StreamState.Complete;

    void IHttpResponseContentFeature.ClearContent()
    {
        if (CurrentStream is { CanSeek: true } body)
        {
            body.SetLength(0);
            return;
        }

        VerifyBuffering();

        _bufferedStream?.Dispose();
        _bufferedStream = null;
    }

    [MemberNotNull(nameof(_factory))]
    private void VerifyBuffering()
    {
        if (_state != StreamState.Buffering)
        {
            throw new InvalidOperationException("Can only clear content if response is buffered.");
        }

        Debug.Assert(_factory is not null);
    }

    ValueTask IHttpResponseBufferingFeature.FlushAsync() => FlushInternalAsync();

    private Stream CurrentStream
    {
        get
        {
            if (_state == StreamState.Buffering)
            {
                VerifyBuffering();
                return _bufferedStream ??= _factory();
            }
            else
            {
                if (_state != StreamState.Complete)
                {
                    _state = StreamState.NotBuffering;
                }

                return _responseBodyFeature.Stream;
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

    public override bool CanRead => false;

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
        if (_state == StreamState.Complete)
        {
            return;
        }

        await FlushInternalAsync();

        _state = StreamState.Complete;

        await _responseBodyFeature.CompleteAsync();
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
