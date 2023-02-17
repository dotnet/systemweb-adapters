// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class RequestHttpApplicationFeature : IHttpApplicationFeature, IHttpResponseEndFeature
{
    private readonly HttpContextCore _context;
    private readonly IHttpResponseEndFeature _previous;

    public RequestHttpApplicationFeature(HttpApplication app, IHttpResponseEndFeature previousEnd, HttpContextCore context)
    {
        Application = app;
        _previous = previousEnd;
        _context = context;
    }

    public RequestNotification CurrentNotification { get; set; }

    public bool IsPostNotification { get; set; }

    public bool IsEnded => _previous.IsEnded;

    public HttpApplication Application { get; }

    public ValueTask RaiseEventAsync(ApplicationEvent @event)
    {
        if (IsEnded)
        {
            return ValueTask.CompletedTask;
        }

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

        Application.InvokeEvent(@event);

        return ValueTask.CompletedTask;
    }

    async Task IHttpResponseEndFeature.EndAsync()
    {
        // Prevent subsequent calls from going through this pathway again
        _context.Features.Set(NoopEndFeature.Instance);

        await RaiseEventAsync(ApplicationEvent.LogRequest);
        await RaiseEventAsync(ApplicationEvent.PostLogRequest);
        await RaiseEventAsync(ApplicationEvent.EndRequest);
        await RaiseEventAsync(ApplicationEvent.PreSendRequestHeaders);

        await _previous.EndAsync();
    }

    private sealed class NoopEndFeature : IHttpResponseEndFeature
    {
        public static IHttpResponseEndFeature Instance { get; } = new NoopEndFeature();

        public bool IsEnded => true;

        public Task EndAsync() => Task.CompletedTask;
    }
}
