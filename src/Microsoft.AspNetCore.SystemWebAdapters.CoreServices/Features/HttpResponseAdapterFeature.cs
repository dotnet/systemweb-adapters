// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

internal class HttpResponseAdapterFeature :
    Stream,
    IHttpResponseBodyFeature,
    IHttpResponseBufferingFeature,
    IHttpResponseEndFeature,
    IHttpResponseContentFeature
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
    private Stream? _filter;

    public HttpResponseAdapterFeature(IHttpResponseBodyFeature httpResponseBody)
    {
        _responseBodyFeature = httpResponseBody;
        _state = StreamState.NotStarted;
    }

    Task IHttpResponseBodyFeature.CompleteAsync() => CompleteAsync();

    void IHttpResponseBodyFeature.DisableBuffering()
    {
        _responseBodyFeature.DisableBuffering();
        _state = StreamState.NotBuffering;

        // If anything is already buffered, we'll use a custom pipe that will
        // clear out the buffer the next time flush is called since this method
        // is not async
        if (_bufferedStream is { })
        {
            _pipeWriter = new FlushingBufferedPipeWriter(this, _responseBodyFeature.Writer);
        }
        else
        {
            _pipeWriter = _responseBodyFeature.Writer;
        }
    }

    void IHttpResponseBufferingFeature.EnableBuffering(int memoryThreshold, long? bufferLimit)
    {
        if (_state == StreamState.Buffering)
        {
            return;
        }
        else if (_state == StreamState.NotStarted)
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
        if (_pipeWriter is { })
        {
            await _pipeWriter.FlushAsync();
        }

        if (_state is StreamState.Buffering)
        {
            await DrainStreamAsync(default);
        }
    }

    private async ValueTask DrainStreamAsync(CancellationToken token)
    {
        if (_bufferedStream is null)
        {
            return;
        }

        if (!SuppressContent)
        {
            if (_filter is { } filter)
            {
                await _bufferedStream.DrainBufferAsync(filter, token);
                await filter.DisposeAsync();
                _filter = null;
            }
            else
            {
                await _bufferedStream.DrainBufferAsync(_responseBodyFeature.Stream, token);
            }
        }

        await _bufferedStream.DisposeAsync();
        _bufferedStream = null;
    }

    Stream IHttpResponseBodyFeature.Stream => this;

    PipeWriter IHttpResponseBodyFeature.Writer
    {
        get
        {
            if (_pipeWriter is null)
            {
                _pipeWriter = PipeWriter.Create(this, new StreamPipeWriterOptions(leaveOpen: true));

                if (_state is StreamState.Complete)
                {
                    _pipeWriter.Complete();
                }
            }

            return _pipeWriter;
        }
    }

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
            throw new InvalidOperationException("Response buffering is required");
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

    [AllowNull]
    Stream IHttpResponseBufferingFeature.Filter
    {
        get
        {
            VerifyBuffering();
            return _filter ?? _responseBodyFeature.Stream;
        }
        set
        {
            VerifyBuffering();
            _filter = value;
        }
    }

    private async Task CompleteAsync()
    {
        if (_state == StreamState.Complete)
        {
            return;
        }

        await FlushInternalAsync();

        _state = StreamState.Complete;

        if (_pipeWriter is { })
        {
            await _pipeWriter.CompleteAsync();
        }

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

    /// <summary>
    /// A <see cref="PipeWriter"/> that can flush any existing buffered items before writing next sequence of bytes
    /// Intended to be used if <see cref="IHttpResponseBodyFeature.DisableBuffering"/> is called and data has been buffered
    /// to ensure that the final output will be ordered correctly (since we can't asynchronously write the data in that call).
    /// </summary>
    /// <remarks>
    /// Calls to <see cref="Advance(int)"/>, <see cref="GetSpan(int)"/>, <see cref="GetMemory(int)"/> must be called
    /// in a group without calling <see cref="FlushAsync(CancellationToken)"/>. If not, then the call to <see cref="Advance(int)"/>
    /// will potentially advance the inner pipe rather than the buffer.
    /// </remarks>
    private sealed class FlushingBufferedPipeWriter : PipeWriter
    {
        private readonly PipeWriter _other;

        private HttpResponseAdapterFeature? _feature;
        private ArrayBufferWriter<byte>? _buffer;

        public FlushingBufferedPipeWriter(HttpResponseAdapterFeature feature, PipeWriter other)
        {
            _feature = feature;
            _other = other;
        }

        public override void CancelPendingFlush() => _other.CancelPendingFlush();

        public override void Complete(Exception? exception = null) => _other.Complete(exception);

        public override async ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
        {
            await FlushExistingDataAsync(cancellationToken);

            return await _other.FlushAsync(cancellationToken);
        }

        private async ValueTask FlushExistingDataAsync(CancellationToken cancellationToken)
        {
            if (_feature is { })
            {
                await _feature.DrainStreamAsync(cancellationToken);
                _feature = null;
            }

            if (_buffer is { })
            {
                await _other.WriteAsync(_buffer.WrittenMemory, cancellationToken);
                _buffer = null;
            }
        }

        public bool IsBuffered => _feature is { };

        public override void Advance(int bytes)
        {
            if (_buffer is { })
            {
                _buffer.Advance(bytes);
            }
            else
            {
                _other.Advance(bytes);
            }
        }

        public override Memory<byte> GetMemory(int sizeHint = 0)
        {
            if (IsBuffered)
            {
                return (_buffer ??= new()).GetMemory(sizeHint);
            }
            else
            {
                return _other.GetMemory(sizeHint);
            }
        }

        public override Span<byte> GetSpan(int sizeHint = 0)
        {
            if (IsBuffered)
            {
                return (_buffer ??= new()).GetSpan(sizeHint);
            }
            else
            {
                return _other.GetSpan(sizeHint);
            }
        }
    }
}
