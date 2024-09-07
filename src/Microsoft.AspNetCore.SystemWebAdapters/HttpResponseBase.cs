// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web
{
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
    public class HttpResponseBase
    {
        public virtual int StatusCode
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual int SubStatusCode
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual string StatusDescription
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual NameValueCollection Headers
        {
            get => throw new NotImplementedException();
        }

        public virtual bool HeadersWritten
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual bool TrySkipIisCustomErrors
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual bool IsRequestBeingRedirected => throw new NotImplementedException();

        public virtual string? ContentType
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual Encoding ContentEncoding
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual bool BufferOutput  => throw new NotImplementedException();

        public virtual Stream OutputStream => throw new NotImplementedException();

        public virtual HttpCookieCollection Cookies => throw new NotImplementedException();

        public virtual void AppendCookie(HttpCookie cookie) => throw new NotImplementedException();

        public virtual bool SuppressContent
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual string Charset
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual TextWriter Output
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        [AllowNull]
        public virtual Stream Filter
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual HttpCachePolicy Cache => throw new NotImplementedException();

        public virtual bool IsClientConnected => throw new NotImplementedException();

        public virtual void AddHeader(string name, string value) => throw new NotImplementedException();

        public virtual void AppendHeader(string name, string value) => throw new NotImplementedException();

        public virtual void SetCookie(HttpCookie cookie) => throw new NotImplementedException();

        [SuppressMessage("Naming", "CA1716:Using a reserved keyword as the name of a virtual/interface member makes it harder for consumers in other languages to override/implement the member", Justification = Constants.ApiFromAspNet)]
        public virtual void End() => throw new NotImplementedException();

        public virtual void Write(char ch) => throw new NotImplementedException();

        public virtual void Write(string s) => throw new NotImplementedException();

        public virtual void Write(object obj) => throw new NotImplementedException();

        public virtual void BinaryWrite(byte[] buffer) => throw new NotImplementedException();

        public virtual void Clear() => throw new NotImplementedException();

        public virtual void ClearContent() => throw new NotImplementedException();

        public virtual void ClearHeaders() => throw new NotImplementedException();

        public virtual void WriteFile(string filename) => throw new NotImplementedException();

        public virtual void TransmitFile(string filename) => throw new NotImplementedException();

        public virtual void TransmitFile(string filename, long offset, long length) => throw new NotImplementedException();

        [return: NotNullIfNotNull(nameof(response))]
        public static implicit operator HttpResponseBase?(HttpResponseCore? response) => response?.HttpContext.AsSystemWebBase().Response;
    }
}
