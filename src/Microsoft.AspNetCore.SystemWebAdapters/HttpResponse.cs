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
        private FeatureReference<IHttpResponseAdapterFeature> _adapterFeature;
        private TextWriter? _writer;
        private HttpCookieCollection? _cookies;

        internal HttpResponse(HttpResponseCore response)
        {
            _response = response;
            _adapterFeature = FeatureReference<IHttpResponseAdapterFeature>.Default;
        }

        private IHttpResponseAdapterFeature AdapterFeature => _adapterFeature.Fetch(_response.HttpContext.Features) ?? throw new InvalidOperationException($"Response buffering must be enabled on this endpoint for this API via the BufferResponseStreamAttribute metadata item");

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

        public Stream OutputStream => _response.Body;

        public HttpCookieCollection Cookies => _cookies ??= new(this);

        public void AppendCookie(HttpCookie cookie) => Cookies.Add(cookie);

        public bool SuppressContent
        {
            get => AdapterFeature.SuppressContent;
            set => AdapterFeature.SuppressContent = value;
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
                    _writer = new StreamWriter(_response.Body, ContentEncoding, leaveOpen: true)
                    {
                        AutoFlush = true,
                    };

                    _response.RegisterForDispose(_writer);
                }

                return _writer;
            }

            set => _writer = value;
        }

        public bool IsClientConnected => !_response.HttpContext.RequestAborted.IsCancellationRequested;

        public void AddHeader(string name, string value) => AppendHeader(name, value);

        public void AppendHeader(string name, string value)
        {
            if (_response.Headers.TryGetValue(name, out var existing))
            {
                _response.Headers[name] = StringValues.Concat(existing, value);
            }
            else
            {
                _response.Headers.Add(name, value);
            }
        }

        public string RedirectLocation
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

            var vdir = _response.HttpContext.RequestServices.GetRequiredService<IHttpRuntime>().AppDomainAppVirtualPath;

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

        public void Flush() => _response.CompleteAsync().GetAwaiter().GetResult();

        public Task FlushAsync() => _response.CompleteAsync();

        public void End() => AdapterFeature.EndAsync().GetAwaiter().GetResult();

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

        public void ClearContent()
        {
            if (_response.Body.CanSeek)
            {
                _response.Body.SetLength(0);
            }
            else
            {
                AdapterFeature.ClearContent();
            }
        }

        public void WriteFile(string filename)
            => TransmitFile(filename);

        public void TransmitFile(string filename)
            => TransmitFile(filename, 0, -1);

        public void TransmitFile(string filename, long offset, long length)
            => _response.SendFileAsync(filename, offset, length >= 0 ? length : null).GetAwaiter().GetResult();

        [return: NotNullIfNotNull("response")]
        public static implicit operator HttpResponse?(HttpResponseCore? response) => response?.GetAdapter();

        [return: NotNullIfNotNull("response")]
        public static implicit operator HttpResponseCore?(HttpResponse? response) => response?._response;
    }
}
