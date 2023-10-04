// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace System.Web
{
    public class HttpResponseWrapper : HttpResponseBase
    {
        private readonly HttpResponse _response;

        public HttpResponseWrapper(HttpResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);

            _response = response;
        }

        public override void AddHeader(string name, string value) => _response.AddHeader(name, value);

        public override void AppendHeader(string name, string value) => _response.AppendHeader(name, value);

        public override string Charset
        {
            get => _response.Charset;
            set => _response.Charset = value;
        }

        public override string? ContentType
        {
            get => _response.ContentType;
            set => _response.ContentType = value;
        }

        public override Encoding ContentEncoding
        {
            get => _response.ContentEncoding;
            set => _response.ContentEncoding = value;
        }

        public override HttpCookieCollection Cookies
        {
            get => _response.Cookies;
        }

        public override NameValueCollection Headers => _response.Headers;

        public override bool HeadersWritten => _response.HeadersWritten;

        public override bool IsClientConnected => _response.IsClientConnected;

        public override TextWriter Output
        {
            get => _response.Output;
            set => _response.Output = value;
        }

        public override bool BufferOutput => _response.BufferOutput;

        public override Stream OutputStream => _response.OutputStream;

        public override void SetCookie(HttpCookie cookie) => _response.SetCookie(cookie);

        public override void AppendCookie(HttpCookie cookie) => _response.AppendCookie(cookie);

        public override int StatusCode
        {
            get => _response.StatusCode;
            set => _response.StatusCode = value;
        }

        public override int SubStatusCode
        {
            get => _response.SubStatusCode;
            set => _response.SubStatusCode = value;
        }

        public override bool IsRequestBeingRedirected => _response.IsRequestBeingRedirected;

        public override string StatusDescription
        {
            get => _response.StatusDescription;
            set => _response.StatusDescription = value;
        }

        public override bool SuppressContent
        {
            get => _response.SuppressContent;
            set => _response.SuppressContent = value;
        }

        public override bool TrySkipIisCustomErrors
        {
            get => _response.TrySkipIisCustomErrors;
            set => _response.TrySkipIisCustomErrors = value;
        }

        public override HttpCachePolicy Cache => _response.Cache;

        [AllowNull]
        public override Stream Filter
        {
            get => _response.Filter;
            set => _response.Filter = value;
        }

        public override void Write(char ch) => _response.Write(ch);

        public override void Write(object obj) => _response.Write(obj);

        public override void Write(string s) => _response.Write(s);

        public override void BinaryWrite(byte[] buffer) => _response.BinaryWrite(buffer);

        public override void Clear() => _response.Clear();

        public override void ClearContent() => _response.ClearContent();

        public override void ClearHeaders() => _response.ClearHeaders();

        public override void End() => _response.End();

        public override void TransmitFile(string filename) => _response.TransmitFile(filename);

        public override void TransmitFile(string filename, long offset, long length) => _response.TransmitFile(filename, offset, length);

        public override void WriteFile(string filename) => _response.WriteFile(filename);
    }
}
