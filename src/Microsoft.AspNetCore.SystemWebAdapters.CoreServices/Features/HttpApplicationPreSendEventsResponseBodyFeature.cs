// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

/// <summary>
/// Used to intercept calls to Flush so we can raise <see cref="ApplicationEvent.PreSendRequestHeaders"/> and <see cref="ApplicationEvent.PreSendRequestContent"/> events
/// </summary>
internal sealed class HttpApplicationPreSendEventsResponseBodyFeature : PipeWriter, IHttpResponseBodyFeature
{
    private State _state;
    private int _byteCount;

    private readonly PipeWriter _pipe;
    private readonly IHttpResponseBodyFeature _other;
    private readonly HttpContextCore _context;

    private enum State
    {
        NotStarted,
        RaisingPreHeader,
        ReadyForContent,
        RaisingPreContent,
    }

    public HttpApplicationPreSendEventsResponseBodyFeature(HttpContextCore context, IHttpResponseBodyFeature other)
    {
        _other = other;
        _pipe = _other.Writer;
        _context = context;
    }

    public Stream Stream => Writer.AsStream();

    public PipeWriter Writer => this;

    Task IHttpResponseBodyFeature.CompleteAsync() => _other.CompleteAsync();

    void IHttpResponseBodyFeature.DisableBuffering() => _other.DisableBuffering();

    public Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken = default)
        => SendFileFallback.SendFileAsync(Stream, path, offset, count, cancellationToken);

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Features.Get<IHttpApplicationFeature>() is { } httpApplication)
        {
            await RaisePreSendRequestHeadersEvent(httpApplication);
        }

        await _other.StartAsync(cancellationToken);
    }

    public override void Advance(int bytes)
    {
        // Don't track additional bytes written when events are being raised or we end up with some recursion
        if (_state is not (State.RaisingPreContent or State.RaisingPreHeader))
        {
            _byteCount += bytes;
        }

        _pipe.Advance(bytes);
    }

    public override void CancelPendingFlush() => _pipe.CancelPendingFlush();

    public override void Complete(Exception? exception = null) => _pipe.Complete(exception);

    public override async ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
    {
        // Only need to raise events if data will be flushed 
        if (_byteCount == 0)
        {
            return default;
        }

        if (_context.Features.Get<IHttpApplicationFeature>() is { } httpApplication)
        {
            _byteCount = 0;

            await RaisePreSendRequestHeadersEvent(httpApplication);
            await RaisePreSendRequestContentEvent(httpApplication);
        }

        return await _pipe.FlushAsync(cancellationToken);
    }

    private async ValueTask RaisePreSendRequestHeadersEvent(IHttpApplicationFeature httpApplication)
    {
        if (_state is State.NotStarted)
        {
            _state = State.RaisingPreHeader;
            await httpApplication.RaiseEventAsync(ApplicationEvent.PreSendRequestHeaders);
            _state = State.ReadyForContent;
        }
    }

    private async ValueTask RaisePreSendRequestContentEvent(IHttpApplicationFeature httpApplication)
    {
        if (_state is State.ReadyForContent)
        {
            _state = State.RaisingPreContent;
            await httpApplication.RaiseEventAsync(ApplicationEvent.PreSendRequestContent);
            _state = State.ReadyForContent;
        }
    }

    public override Memory<byte> GetMemory(int sizeHint = 0) => _pipe.GetMemory(sizeHint);

    public override Span<byte> GetSpan(int sizeHint = 0) => _pipe.GetSpan(sizeHint);
}
