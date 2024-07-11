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
using Microsoft.AspNetCore.SystemWebAdapters.Features;
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

        private NameValueCollection? _headers;
        private ResponseHeaders? _typedHeaders;
        private TextWriter? _writer;
        private HttpCookieCollection? _cookies;
        private HttpCachePolicy? _cache;

        internal HttpResponse(HttpResponseCore response)
        {
            Response = response;
        }

        internal HttpResponseCore Response { get; }

        internal ResponseHeaders TypedHeaders => _typedHeaders ??= new(Response.Headers);

        public int StatusCode
        {
            get => Response.StatusCode;
            set => Response.StatusCode = value;
        }

        public int SubStatusCode { get; set; }

        [AllowNull]
        public string StatusDescription
        {
            get => Response.HttpContext.Features.GetRequiredFeature<IHttpResponseFeature>().ReasonPhrase ?? ReasonPhrases.GetReasonPhrase(Response.StatusCode);
            set => Response.HttpContext.Features.GetRequiredFeature<IHttpResponseFeature>().ReasonPhrase = value;
        }

        public NameValueCollection Headers => _headers ??= Response.Headers.ToNameValueCollection();

        public void ClearHeaders()
        {
            Response.Headers.Clear();
            _cookies?.Clear();

            StatusCode = 200;
            SubStatusCode = 0;
            StatusDescription = null;
            ContentType = "text/html";
            Charset = Encoding.UTF8.WebName;
        }

        public bool TrySkipIisCustomErrors
        {
            get => Response.HttpContext.Features.GetRequiredFeature<IStatusCodePagesFeature>().Enabled;
            set => Response.HttpContext.Features.GetRequiredFeature<IStatusCodePagesFeature>().Enabled = value;
        }

        public bool BufferOutput => Response.HttpContext.Features.GetRequiredFeature<IHttpResponseBufferingFeature>().IsEnabled;

        public Stream OutputStream => Response.Body;

        [AllowNull]
        public Stream Filter
        {
            get => Response.HttpContext.Features.GetRequiredFeature<IHttpResponseBufferingFeature>().Filter;
            set => Response.HttpContext.Features.GetRequiredFeature<IHttpResponseBufferingFeature>().Filter = value;
        }

        public HttpCookieCollection Cookies => _cookies ??= new(this);

        public void AppendCookie(HttpCookie cookie) => Cookies.Add(cookie);

        public bool SuppressContent
        {
            get => Response.HttpContext.Features.GetRequiredFeature<IHttpResponseContentFeature>().SuppressContent;
            set => Response.HttpContext.Features.GetRequiredFeature<IHttpResponseContentFeature>().SuppressContent = value;
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
                    _writer = new StreamWriter(Response.Body, ContentEncoding, leaveOpen: true)
                    {
                        AutoFlush = true,
                    };
                }

                return _writer;
            }

            set => _writer = value;
        }

        public bool IsClientConnected => !Response.HttpContext.RequestAborted.IsCancellationRequested;

        public void AddHeader(string name, string value) => AppendHeader(name, value);

        public void AppendHeader(string name, string value) => Response.Headers.Append(name, value);

        public bool HeadersWritten
        {
            get => Response.HasStarted;
        }

        public string? RedirectLocation
        {
            get => Response.Headers.Location;
            set => Response.Headers.Location = value;
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
            Response.Redirect(resolved, permanent);

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

        public HttpCachePolicy Cache => _cache ??= new(Response.HttpContext);

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

            var vdir = Response.HttpContext.RequestServices.GetRequiredService<IOptions<SystemWebAdaptersOptions>>().Value.AppDomainAppVirtualPath;

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

        public void Flush() => Response.Body.Flush();

        public Task FlushAsync() => Response.Body.FlushAsync(Response.HttpContext.RequestAborted);

        public void End() => Response.HttpContext.Features.GetRequiredFeature<IHttpResponseEndFeature>().EndAsync().GetAwaiter().GetResult();

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
            Response.Clear();
            ClearContent();
        }

        public void ClearContent() => Response.HttpContext.Features.GetRequiredFeature<IHttpResponseContentFeature>().ClearContent();

        public void WriteFile(string filename)
            => TransmitFile(filename);

        public void TransmitFile(string filename)
            => TransmitFile(filename, 0, -1);

        public void TransmitFile(string filename, long offset, long length)
            => Response.SendFileAsync(filename, offset, length >= 0 ? length : null).GetAwaiter().GetResult();

        [return: NotNullIfNotNull(nameof(response))]
        public static implicit operator HttpResponse?(HttpResponseCore? response) => response?.HttpContext.AsSystemWeb().Response;

        [return: NotNullIfNotNull(nameof(response))]
        public static implicit operator HttpResponseCore?(HttpResponse? response) => response?.AsAspNetCore();
    }
}
