// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class RequestHttpApplicationFeature : IHttpApplicationFeature, IHttpResponseEndFeature
{
    private readonly IHttpResponseEndFeature _previous;
    private State _state;
    private List<Exception>? _exceptions;

    public RequestHttpApplicationFeature(HttpApplication app, IHttpResponseEndFeature previousEnd)
    {
        Application = app;
        _previous = previousEnd;
    }

    public RequestNotification CurrentNotification { get; set; }

    public bool IsPostNotification { get; set; }

    public bool IsEnded => _state != State.Running;

    public HttpApplication Application { get; }

    ValueTask IHttpApplicationFeature.RaiseEventAsync(ApplicationEvent @event)
    {
        if (IsEnded)
        {
            return ValueTask.CompletedTask;
        }

        return RaiseEventAsync(@event, true);
    }

    public ValueTask RaiseEventAsync(ApplicationEvent @event, bool @throw)
    {
        (CurrentNotification, IsPostNotification) = @event switch
        {
            ApplicationEvent.BeginRequest => (RequestNotification.BeginRequest, false),
            ApplicationEvent.AuthenticateRequest => (RequestNotification.AuthenticateRequest, false),
            ApplicationEvent.PostAuthenticateRequest => (RequestNotification.AuthenticateRequest, true),
            ApplicationEvent.AuthorizeRequest => (RequestNotification.AuthorizeRequest, false),
            ApplicationEvent.PostAuthorizeRequest => (RequestNotification.AuthorizeRequest, true),
            ApplicationEvent.ResolveRequestCache => (RequestNotification.ResolveRequestCache, false),
            ApplicationEvent.PostResolveRequestCache => (RequestNotification.ResolveRequestCache, true),
            ApplicationEvent.MapRequestHandler => (RequestNotification.MapRequestHandler, false),
            ApplicationEvent.PostMapRequestHandler => (RequestNotification.MapRequestHandler, true),
            ApplicationEvent.AcquireRequestState => (RequestNotification.AcquireRequestState, false),
            ApplicationEvent.PostAcquireRequestState => (RequestNotification.AcquireRequestState, true),
            ApplicationEvent.PreRequestHandlerExecute => (RequestNotification.PreExecuteRequestHandler, false),
            ApplicationEvent.PostRequestHandlerExecute => (RequestNotification.ExecuteRequestHandler, true),
            ApplicationEvent.ReleaseRequestState => (RequestNotification.ReleaseRequestState, false),
            ApplicationEvent.PostReleaseRequestState => (RequestNotification.ReleaseRequestState, true),
            ApplicationEvent.UpdateRequestCache => (RequestNotification.UpdateRequestCache, false),
            ApplicationEvent.PostUpdateRequestCache => (RequestNotification.UpdateRequestCache, true),
            ApplicationEvent.LogRequest => (RequestNotification.LogRequest, false),
            ApplicationEvent.PostLogRequest => (RequestNotification.LogRequest, true),
            ApplicationEvent.EndRequest => (RequestNotification.EndRequest, false),

            // Remaining events just continue using the existing notifications
            _ => (CurrentNotification, IsPostNotification),
        };

        try
        {
            Application.InvokeEvent(@event);
        }
        catch (Exception ex)
        {
            Application.InvokeEvent(ApplicationEvent.Error);
            AddError(ex);

            if (@throw)
            {
                throw;
            }
        }

        return ValueTask.CompletedTask;
    }

    async Task IHttpResponseEndFeature.EndAsync()
    {
        if (_state != State.Running)
        {
            return;
        }

        _state = State.Ending;

        await RaiseEventAsync(ApplicationEvent.LogRequest, false);
        await RaiseEventAsync(ApplicationEvent.PostLogRequest, false);
        await RaiseEventAsync(ApplicationEvent.EndRequest, false);
        await RaiseEventAsync(ApplicationEvent.PreSendRequestHeaders, false);

        _state = State.Ended;

        await _previous.EndAsync();

        if (_exceptions is [{ } exception])
        {
            throw exception;
        }
        else if (_exceptions is { } exceptions)
        {
            throw new AggregateException(exceptions);
        }
    }
    private void AddError(Exception ex)
        => (_exceptions ??= new()).Add(ex);

    private enum State
    {
        Running,
        Ending,
        Ended
    }
}
