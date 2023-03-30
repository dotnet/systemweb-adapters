// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.SessionState;

namespace System.Web
{
    public class HttpContextWrapper : HttpContextBase
    {
        private readonly HttpContext _context;

        private HttpRequestBase? _request;
        private HttpResponseBase? _response;
        private HttpSessionStateBase? _session;

        public HttpContextWrapper(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            _context = httpContext;
        }

        public override DateTime Timestamp => _context.Timestamp;

        public override IDictionary Items => _context.Items;

        public override bool IsDebuggingEnabled => _context.IsDebuggingEnabled;

        public override void AddError(Exception ex) => _context.AddError(ex);

        public override Exception[] AllErrors => _context.AllErrors;

        public override void ClearError() => _context.ClearError();

        public override Exception? Error => _context.Error;

        public override HttpRequestBase Request => _request ??= new HttpRequestWrapper(_context.Request);

        public override HttpResponseBase Response => _response ??= new HttpResponseWrapper(_context.Response);

        public override HttpApplication ApplicationInstance => _context.ApplicationInstance;

        public override HttpApplicationState Application => _context.Application;

        public override RequestNotification CurrentNotification => _context.CurrentNotification;

        public override bool IsPostNotification => _context.IsPostNotification;

        public override HttpServerUtilityBase Server => new HttpServerUtilityWrapper(_context.Server);

        public override HttpSessionStateBase? Session
        {
            get
            {
                if (_session is null && _context.Session is HttpSessionState sessionState)
                {
                    _session = new HttpSessionStateWrapper(sessionState);
                }

                return _session;
            }
        }

        public override IPrincipal User
        {
            get => _context.User;
            set => _context.User = value;
        }

        public override Cache Cache => _context.Cache;
    }
}
