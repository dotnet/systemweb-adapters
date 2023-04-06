// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using System.Text;

namespace System.Web
{
    public class HttpRequestWrapper : HttpRequestBase
    {
        private readonly HttpRequest _request;

        public HttpRequestWrapper(HttpRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            _request = request;
        }

        public override string[] AcceptTypes => _request.AcceptTypes;

        public override void Abort() => _request.Abort();

        public override byte[] BinaryRead(int count) => _request.BinaryRead(count);

        public override Encoding? ContentEncoding => _request.ContentEncoding;

        public override int ContentLength => _request.ContentLength;

        public override string? ContentType
        {
            get => _request.ContentType;
            set => _request.ContentType = value;
        }

        public override HttpCookieCollection Cookies => _request.Cookies;

        public override NameValueCollection Headers => _request.Headers;

        public override HttpFileCollectionBase Files => new HttpFileCollectionWrapper(_request.Files);

        public override NameValueCollection Form => _request.Form;

        public override string HttpMethod => _request.HttpMethod;

        public override Stream InputStream => _request.InputStream;

        public override Stream GetBufferedInputStream() => _request.GetBufferedInputStream();

        public override Stream GetBufferlessInputStream() => _request.GetBufferlessInputStream();

        public override bool IsAuthenticated => _request.IsAuthenticated;

        public override bool IsLocal => _request.IsLocal;

        public override IIdentity? LogonUserIdentity => _request.LogonUserIdentity;

        public override string? Path => _request.Path;

        public override NameValueCollection QueryString => _request.QueryString;

        public override HttpBrowserCapabilitiesBase Browser => new HttpBrowserCapabilitiesWrapper(_request.Browser);

        public override string? RawUrl => _request.RawUrl;

        public override string RequestType => _request.RequestType;

        public override int TotalBytes => _request.TotalBytes;

        public override Uri Url => _request.Url;

        public override Uri? UrlReferrer => _request.UrlReferrer;

        public override string UserAgent => _request.UserAgent;

        public override string? UserHostAddress => _request.UserHostAddress;

        public override string? UserHostName => _request.UserHostName;

        public override string[] UserLanguages => _request.UserLanguages;

        public override string ApplicationPath => _request.ApplicationPath;

        public override string AppRelativeCurrentExecutionFilePath => _request.AppRelativeCurrentExecutionFilePath;

        public override bool IsSecureConnection => _request.IsSecureConnection;

        public override NameValueCollection ServerVariables => _request.ServerVariables;

        public override NameValueCollection Params => _request.Params;

        public override string? this[string key] => _request[key];
    }
}
