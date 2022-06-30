// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using System.Text;

namespace System.Web
{
    [Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
    public abstract class HttpRequestBase
    {
        public virtual string? Path => throw new NotImplementedException();

        public virtual NameValueCollection Headers => throw new NotImplementedException();

        public virtual Uri Url => throw new NotImplementedException();

        [Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = Constants.ApiFromAspNet)]
        public virtual string? RawUrl => throw new NotImplementedException();

        public virtual string HttpMethod => throw new NotImplementedException();

        public virtual string? UserHostAddress => throw new NotImplementedException();

        [Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = Constants.ApiFromAspNet)]
        public virtual string[] UserLanguages => throw new NotImplementedException();

        public virtual string UserAgent => throw new NotImplementedException();

        public virtual string RequestType => HttpMethod;

        public virtual HttpCookieCollection Cookies => throw new NotImplementedException();

        public virtual int ContentLength => throw new NotImplementedException();

        public virtual string? ContentType
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual Stream InputStream => throw new NotImplementedException();

        public NameValueCollection ServerVariables => throw new NotImplementedException();

        public bool IsSecureConnection => throw new NotImplementedException();

        public virtual NameValueCollection QueryString => throw new NotImplementedException();

        public virtual bool IsLocal => throw new NotImplementedException();

        public string AppRelativeCurrentExecutionFilePath => throw new NotImplementedException();

        public string ApplicationPath => throw new NotImplementedException();

        public virtual Uri? UrlReferrer => throw new NotImplementedException();

        public virtual int TotalBytes => throw new NotImplementedException();

        public virtual bool IsAuthenticated => throw new NotImplementedException();

        public virtual IIdentity? LogonUserIdentity => throw new NotImplementedException();

        public virtual Encoding? ContentEncoding => throw new NotImplementedException();

        public virtual string? UserHostName => throw new NotImplementedException();

        public virtual HttpBrowserCapabilitiesBase Browser => throw new NotImplementedException();

        public virtual byte[] BinaryRead(int count) => throw new NotImplementedException();

        public virtual void Abort() => throw new NotImplementedException();
    }
}
