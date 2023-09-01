// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Internal;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace System.Web
{
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "_writer is registered to be disposed by the owning HttpContext")]
    public class HttpResponse
    {
        private const string NoContentTypeMessage = "No content type declared";

        private readonly HttpResponseCore _response;

        private NameValueCollection? _headers;
        private ResponseHeaders? _typedHeaders;
        private TextWriter? _writer;
        private HttpCookieCollection? _cookies;
        private HttpCachePolicy? _cache;

        internal HttpResponse(HttpResponseCore response)
        {
            _response = response;
        }

        internal ResponseHeaders TypedHeaders => _typedHeaders ??= new(_response.Headers);

        public int StatusCode
        {
            get => _response.StatusCode;
            set => _response.StatusCode = value;
        }

        public int SubStatusCode { get; set; }

        [AllowNull]
        public string StatusDescription
        {
            get => _response.HttpContext.Features.GetRequired<IHttpResponseFeature>().ReasonPhrase ?? ReasonPhrases.GetReasonPhrase(_response.StatusCode);
            set => _response.HttpContext.Features.GetRequired<IHttpResponseFeature>().ReasonPhrase = value;
        }

        public NameValueCollection Headers => _headers ??= _response.Headers.ToNameValueCollection();

        public void ClearHeaders()
        {
            _response.Headers.Clear();
            _cookies?.Clear();

            StatusCode = 200;
            SubStatusCode = 0;
            StatusDescription = null;
            ContentType = "text/html";
            Charset = Encoding.UTF8.WebName;
        }

        public bool TrySkipIisCustomErrors
        {
            get => _response.HttpContext.Features.GetRequired<IStatusCodePagesFeature>().Enabled;
            set => _response.HttpContext.Features.GetRequired<IStatusCodePagesFeature>().Enabled = value;
        }

        public bool BufferOutput => _response.HttpContext.Features.GetRequired<IHttpResponseBufferingFeature>().IsEnabled;

        public Stream OutputStream => _response.Body;

        [AllowNull]
        public Stream Filter
        {
            get => _response.HttpContext.Features.GetRequired<IHttpResponseBufferingFeature>().Filter;
            set => _response.HttpContext.Features.GetRequired<IHttpResponseBufferingFeature>().Filter = value;
        }

        public HttpCookieCollection Cookies => _cookies ??= new(this);

        public void AppendCookie(HttpCookie cookie) => Cookies.Add(cookie);

        public bool SuppressContent
        {
            get => _response.HttpContext.Features.GetRequired<IHttpResponseContentFeature>().SuppressContent;
            set => _response.HttpContext.Features.GetRequired<IHttpResponseContentFeature>().SuppressContent = value;
        }

        public Encoding ContentEncoding
        {
            get => TypedHeaders.ContentType?.Encoding ?? Encoding.UTF8;
            set
            {
                if (TypedHeaders.ContentType is { } contentType)
                {
                    if (contentType.Encoding == value)
                    {
                        return;
                    }

                    contentType.Encoding = value;
                    TypedHeaders.ContentType = contentType;

                    // Reset the writer for change in encoding
                    _writer = null;
                }
                else
                {
                    throw new InvalidOperationException(NoContentTypeMessage);
                }
            }
        }

        public string? ContentType
        {
            get => TypedHeaders.ContentType?.MediaType.ToString();
            set
            {
                if (TypedHeaders.ContentType is { } contentType)
                {
                    contentType.MediaType = value;
                    TypedHeaders.ContentType = contentType;
                }
                else
                {
                    TypedHeaders.ContentType = new(value);
                }
            }
        }

        public string Charset
        {
            get => TypedHeaders.ContentType?.Charset.Value ?? Encoding.UTF8.WebName;
            set
            {
                if (TypedHeaders.ContentType is { } contentType)
                {
                    contentType.Charset = value;
                    TypedHeaders.ContentType = contentType;
                }
                else
                {
                    throw new InvalidOperationException(NoContentTypeMessage);
                }
            }
        }

        public TextWriter Output
        {
            get
            {
                if (_writer is null)
                {
                    // No need to dispose the stream writer as it doesn't own the stream and autoflushes
                    _writer = new StreamWriter(_response.Body, ContentEncoding, leaveOpen: true)
                    {
                        AutoFlush = true,
                    };
                }

                return _writer;
            }

            set => _writer = value;
        }

        public bool IsClientConnected => !_response.HttpContext.RequestAborted.IsCancellationRequested;

        public void AddHeader(string name, string value) => AppendHeader(name, value);

        public void AppendHeader(string name, string value) => _response.Headers.Append(name, value);

        public bool HeadersWritten
        {
            get => _response.HasStarted;
        }

        public string? RedirectLocation
        {
            get => _response.Headers.Location;
            set => _response.Headers.Location = value;
        }

        public bool IsRequestBeingRedirected => StatusCode is >= 300 and < 400;

        [SuppressMessage("Design", "CA1054:URI parameters should not be strings", Justification = Constants.ApiFromAspNet)]
        public void Redirect(string url) => Redirect(url, endResponse: true, permanent: false);

        [SuppressMessage("Design", "CA1054:URI parameters should not be strings", Justification = Constants.ApiFromAspNet)]
        public void Redirect(string url, bool endResponse) => Redirect(url, endResponse, permanent: false);

        [SuppressMessage("Design", "CA1054:URI parameters should not be strings", Justification = Constants.ApiFromAspNet)]
        public void RedirectPermanent(string url) => Redirect(url, endResponse: true, permanent: true);

        [SuppressMessage("Design", "CA1054:URI parameters should not be strings", Justification = Constants.ApiFromAspNet)]
        public void RedirectPermanent(string url, bool endResponse) => Redirect(url, endResponse, permanent: true);

        private void Redirect(string url, bool endResponse, bool permanent)
        {
            Clear();

            var resolved = ResolvePath(url);
            _response.Redirect(resolved, permanent);

            ContentType = "text/html";

            Output.WriteLine("<html><head><title>Object moved</title></head><body>");
            Output.Write("<h2>Object moved to <a href=\"");
            Output.Write(resolved);
            Output.WriteLine("\">here</a>.</h2>");
            Output.WriteLine("</body></html>");

            if (endResponse)
            {
                End();
            }
        }

        public HttpCachePolicy Cache => _cache ??= new(_response.HttpContext);

        private string ResolvePath(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return "/";
            }

            if (!url.StartsWith('~'))
            {
                return url;
            }

            var vdir = _response.HttpContext.RequestServices.GetRequiredService<IOptions<SystemWebAdaptersOptions>>().Value.AppDomainAppVirtualPath;

            var sb = new StringBuilder(url, 1, url.Length - 1, url.Length + vdir.Length);

            if (sb.Length == 0 || sb[0] != '/')
            {
                sb.Insert(0, '/');
            }

            sb.Insert(0, vdir);
            sb.Replace("//", "/");

            return sb.ToString();
        }

        public void SetCookie(HttpCookie cookie) => Cookies.Set(cookie);

        public void Flush() => _response.Body.Flush();

        public Task FlushAsync() => _response.Body.FlushAsync(_response.HttpContext.RequestAborted);

        public void End() => _response.HttpContext.Features.GetRequired<IHttpResponseEndFeature>().EndAsync().GetAwaiter().GetResult();

        public void Write(char ch) => Output.Write(ch);

        public void Write(string s) => Output.Write(s);

        public void Write(object obj) => Output.Write(obj);

        public void BinaryWrite(byte[] buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            OutputStream.Write(buffer, 0, buffer.Length);
        }

        public void Clear()
        {
            _response.Clear();
            ClearContent();
        }

        public void ClearContent() => _response.HttpContext.Features.GetRequired<IHttpResponseContentFeature>().ClearContent();

        public void WriteFile(string filename)
            => TransmitFile(filename);

        public void TransmitFile(string filename)
            => TransmitFile(filename, 0, -1);

        public void TransmitFile(string filename, long offset, long length)
            => _response.SendFileAsync(filename, offset, length >= 0 ? length : null).GetAwaiter().GetResult();

        [return: NotNullIfNotNull(nameof(response))]
        public static implicit operator HttpResponse?(HttpResponseCore? response) => response?.GetAdapter();

        [return: NotNullIfNotNull(nameof(response))]
        public static implicit operator HttpResponseCore?(HttpResponse? response) => response?._response;
    }
}
