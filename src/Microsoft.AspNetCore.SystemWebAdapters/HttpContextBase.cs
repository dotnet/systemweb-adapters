// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Security.Principal;
using System.Diagnostics.CodeAnalysis;
using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web
{
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
    public class HttpContextBase : IServiceProvider
    {
        protected HttpContextBase()
        {
        }

        public virtual HttpRequestBase Request => throw new NotImplementedException();

        public virtual HttpResponseBase Response => throw new NotImplementedException();

        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = Constants.ApiFromAspNet)]
        public virtual Exception? Error => throw new NotImplementedException();

        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = Constants.ApiFromAspNet)]
        public virtual Exception[] AllErrors => throw new NotImplementedException();

        public virtual void ClearError() => throw new NotImplementedException();

        public virtual void AddError(Exception ex) => throw new NotImplementedException();

        public virtual HttpApplication ApplicationInstance => throw new NotImplementedException();

        public virtual HttpApplicationState Application => throw new NotImplementedException();

        public virtual bool IsPostNotification => throw new NotImplementedException();

        public virtual RequestNotification CurrentNotification => throw new NotImplementedException();

        public virtual IDictionary Items => throw new NotImplementedException();

        public virtual DateTime Timestamp => throw new NotImplementedException();

        public virtual IPrincipal User
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual bool IsDebuggingEnabled => throw new NotImplementedException();

        public virtual HttpServerUtilityBase Server => throw new NotImplementedException();

        public virtual HttpSessionStateBase? Session => throw new NotImplementedException();

        public virtual object? GetService(Type serviceType) => throw new NotImplementedException();

        [return: NotNullIfNotNull(nameof(context))]
        public static implicit operator HttpContextBase?(HttpContextCore? context) => context?.GetSystemWebHttpContextBase();

        public virtual Caching.Cache Cache => throw new NotImplementedException();

        public virtual void RewritePath(string path) => throw new NotImplementedException();

        public virtual void RewritePath(string path, bool rebaseClientPath) => throw new NotImplementedException();

        public virtual void RewritePath(string filePath, string pathInfo, string? queryString) => throw new NotImplementedException();

        public virtual void RewritePath(string filePath, string pathInfo, string? queryString, bool setClientFilePath) => throw new NotImplementedException();

        public virtual void SetSessionStateBehavior(SessionStateBehavior sessionStateBehavior) => throw new NotImplementedException();
    }
}
