using System;
using System.Web;

#nullable enable

namespace ModulesLibrary
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1510:Use ArgumentNullException throw helper", Justification = "Source shared with .NET Framework that does not have the method")]
    public abstract class BaseModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication application)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.AcquireRequestState += (s, e) => WriteDetails(s, nameof(application.AcquireRequestState));
            application.AuthenticateRequest += (s, e) => WriteDetails(s, nameof(application.AuthenticateRequest));
            application.AuthorizeRequest += (s, e) => WriteDetails(s, nameof(application.AuthorizeRequest));
            application.BeginRequest += (s, e) => WriteDetails(s, nameof(application.BeginRequest));
            application.EndRequest += (s, e) => WriteDetails(s, nameof(application.EndRequest));
            application.Error += (s, e) => WriteDetails(s, nameof(application.Error));
            application.LogRequest += (s, e) => WriteDetails(s, nameof(application.LogRequest));
            application.MapRequestHandler += (s, e) => WriteDetails(s, nameof(application.MapRequestHandler));
            application.PostAcquireRequestState += (s, e) => WriteDetails(s, nameof(application.PostAcquireRequestState));
            application.PostAuthenticateRequest += (s, e) => WriteDetails(s, nameof(application.PostAuthenticateRequest));
            application.PostAuthorizeRequest += (s, e) => WriteDetails(s, nameof(application.PostAuthorizeRequest));
            application.PostLogRequest += (s, e) => WriteDetails(s, nameof(application.PostLogRequest));
            application.PostMapRequestHandler += (s, e) => WriteDetails(s, nameof(application.PostMapRequestHandler));
            application.PostReleaseRequestState += (s, e) => WriteDetails(s, nameof(application.PostReleaseRequestState));
            application.PostRequestHandlerExecute += (s, e) => WriteDetails(s, nameof(application.PostRequestHandlerExecute));
            application.PostResolveRequestCache += (s, e) => WriteDetails(s, nameof(application.PostResolveRequestCache));
            application.PostUpdateRequestCache += (s, e) => WriteDetails(s, nameof(application.PostUpdateRequestCache));
            application.PreRequestHandlerExecute += (s, e) => WriteDetails(s, nameof(application.PreRequestHandlerExecute));
            application.PreSendRequestHeaders += (s, e) => WriteDetails(s, nameof(application.PreSendRequestHeaders));
            application.ReleaseRequestState += (s, e) => WriteDetails(s, nameof(application.ReleaseRequestState));
            application.ResolveRequestCache += (s, e) => WriteDetails(s, nameof(application.ResolveRequestCache));
            application.UpdateRequestCache += (s, e) => WriteDetails(s, nameof(application.UpdateRequestCache));
        }

        private void WriteDetails(object? sender, string name)
        {
            if (sender is HttpApplication { Context: { } context })
            {
                InvokeEvent(context, name);
            }
        }

        protected abstract void InvokeEvent(HttpContext context, string name);
    }
}
