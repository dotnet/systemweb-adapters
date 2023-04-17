// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class RequestHttpApplicationFeature : IHttpApplicationFeature, IHttpResponseEndFeature, IRequestExceptionFeature
{
    private readonly IHttpResponseEndFeature _previous;
    private List<Exception>? _exceptions;

    public RequestHttpApplicationFeature(HttpApplication app, IHttpResponseEndFeature previousEnd)
    {
        Application = app;
        _previous = previousEnd;
    }

    public RequestNotification CurrentNotification { get; set; }

    public bool IsPostNotification { get; set; }

    public bool IsEnded { get; private set; }

    public HttpApplication Application { get; }

    ValueTask IHttpApplicationFeature.RaiseEventAsync(ApplicationEvent @event)
    {
        RaiseEvent(@event, suppressThrow: false);
        return ValueTask.CompletedTask;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Must handle all exceptions here")]
    private void RaiseEvent(ApplicationEvent appEvent, bool suppressThrow)
    {
        (CurrentNotification, IsPostNotification) = appEvent switch
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
            Application.InvokeEvent(appEvent);
        }
        catch (Exception ex)
        {
            AddError(ex);
            Application.InvokeEvent(ApplicationEvent.Error);

            if (!suppressThrow)
            {
                ThrowIfErrors();
            }
        }
    }

    private void ThrowIfErrors()
    {
        if (_exceptions is [{ } exception])
        {
            throw exception;
        }
        else if (_exceptions is { } exceptions)
        {
            throw new AggregateException(exceptions);
        }
    }

    async Task IHttpResponseEndFeature.EndAsync()
    {
        if (IsEnded)
        {
            return;
        }

        IsEnded = true;

        RaiseEvent(ApplicationEvent.LogRequest, suppressThrow: true);
        RaiseEvent(ApplicationEvent.PostLogRequest, suppressThrow: true);
        RaiseEvent(ApplicationEvent.EndRequest, suppressThrow: true);
        RaiseEvent(ApplicationEvent.PreSendRequestHeaders, suppressThrow: true);

        await _previous.EndAsync();

        ThrowIfErrors();
    }

    private void AddError(Exception ex) => (_exceptions ??= new()).Add(ex);

    void IRequestExceptionFeature.Add(Exception exception) => AddError(exception);

    IReadOnlyList<Exception> IRequestExceptionFeature.Exceptions
        => ((IReadOnlyList<Exception>?)_exceptions) ?? Array.Empty<Exception>();

    void IRequestExceptionFeature.Clear() => _exceptions = null;
}
