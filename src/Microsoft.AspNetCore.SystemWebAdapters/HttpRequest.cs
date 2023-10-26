// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace System.Web
{
    public class HttpRequest
    {
        private readonly HttpRequestCore _request;

        private RequestHeaders? _typedHeaders;
        private string[]? _userLanguages;
        private string[]? _acceptTypes;
        private NameValueCollection? _headers;
        private NameValueCollection? _serverVariables;
        private NameValueCollection? _form;
        private NameValueCollection? _query;
        private HttpFileCollection? _files;
        private HttpCookieCollection? _cookies;
        private NameValueCollection? _params;
        private HttpBrowserCapabilities? _browser;

        internal HttpRequest(HttpRequestCore request)
        {
            _request = request;
        }

        internal RequestHeaders TypedHeaders => _typedHeaders ??= new(_request.Headers);

        public string Path => _request.HttpContext.Features.GetRequired<IHttpRequestPathFeature>().Path;

        public string PathInfo => _request.HttpContext.Features.GetRequired<IHttpRequestPathFeature>().PathInfo;

        public string FilePath => _request.HttpContext.Features.GetRequired<IHttpRequestPathFeature>().FilePath;

        [SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = Constants.ApiFromAspNet)]
        public string RawUrl => _request.HttpContext.Features.GetRequired<IHttpRequestPathFeature>().RawUrl;

        public string? PhysicalPath => _request.HttpContext.Features.GetRequired<IHttpRequestPathFeature>().PhysicalPath;

        public string CurrentExecutionFilePath => _request.HttpContext.Features.GetRequired<IHttpRequestPathFeature>().CurrentExecutionFilePath;

        public NameValueCollection Headers => _headers ??= _request.Headers.ToNameValueCollection();

        public Uri Url => new(_request.GetEncodedUrl());

        public ReadEntityBodyMode ReadEntityBodyMode => _request.HttpContext.Features.GetRequired<IHttpRequestInputStreamFeature>().Mode;

        public Stream GetBufferlessInputStream() => _request.HttpContext.Features.GetRequired<IHttpRequestInputStreamFeature>().GetBufferlessInputStream();

        public Stream GetBufferedInputStream() => _request.HttpContext.Features.GetRequired<IHttpRequestInputStreamFeature>().GetBufferedInputStream();

        public string HttpMethod => _request.Method;

        public string? UserHostAddress => _request.HttpContext.Connection.RemoteIpAddress?.ToString();

        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = Constants.ApiFromAspNet)]
        public string[] UserLanguages
        {
            get
            {
                if (_userLanguages is null)
                {
                    var languages = TypedHeaders.AcceptLanguage;
                    var length = languages.Count;

                    if (length == 0)
                    {
                        _userLanguages = Array.Empty<string>();
                    }
                    else
                    {
                        var qualityArray = ArrayPool<StringWithQualityHeaderValue>.Shared.Rent(length);
                        var userLanguages = new string[length];

                        languages.CopyTo(qualityArray, 0);
                        Array.Sort(qualityArray, 0, length, StringWithQualityHeaderValueComparer.Instance);

                        for (var i = 0; i < length; i++)
                        {
                            if (qualityArray[i].Value.Value is { } language)
                            {
                                userLanguages[i] = language;
                            }
                        }

                        ArrayPool<StringWithQualityHeaderValue>.Shared.Return(qualityArray);

                        _userLanguages = userLanguages;
                    }
                }

                return _userLanguages;
            }
        }

        public string? UserAgent => _request.Headers[HeaderNames.UserAgent];

        public string RequestType => HttpMethod;

        public NameValueCollection Form => _form ??= _request.HasFormContentType ? _request.Form.ToNameValueCollection() : StringValuesReadOnlyDictionaryNameValueCollection.Empty;

        public HttpCookieCollection Cookies => _cookies ??= new(_request.Cookies);

        public HttpFileCollection Files => _files ??= _request.HasFormContentType ? new(_request.Form.Files) : HttpFileCollection.Empty;

        public int ContentLength => (int)(_request.ContentLength ?? 0);

        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = Constants.ApiFromAspNet)]
        public string[] AcceptTypes
        {
            get
            {
                if (_acceptTypes is null)
                {
                    var accept = TypedHeaders.Accept;

                    if (accept.Count == 0)
                    {
                        _acceptTypes = Array.Empty<string>();
                    }
                    else
                    {
                        _acceptTypes = new string[accept.Count];

                        for (var i = 0; i < accept.Count; i++)
                        {
                            if (accept[i].MediaType.Value is { } value)
                            {
                                _acceptTypes[i] = value;
                            }
                        }
                    }
                }

                return _acceptTypes;
            }
        }

        public string? ContentType
        {
            get => _request.ContentType;
            set => _request.ContentType = value;
        }

        public Stream InputStream => _request.HttpContext.Features.GetRequired<IHttpRequestInputStreamFeature>().InputStream;

        public NameValueCollection ServerVariables => _serverVariables ??= _request.HttpContext.Features.GetRequired<IServerVariablesFeature>().ToNameValueCollection();

        public bool IsSecureConnection => _request.IsHttps;

        public NameValueCollection QueryString => _query ??= _request.Query.ToNameValueCollection();

        public bool IsLocal
        {
            get
            {
                var connectionInfo = _request.HttpContext.Connection;

                // If unknown, assume not local
                if (connectionInfo.RemoteIpAddress is null)
                {
                    return false;
                }

                // Check if localhost
                if (IPAddress.IsLoopback(connectionInfo.RemoteIpAddress))
                {
                    return true;
                }

                return connectionInfo.RemoteIpAddress.Equals(connectionInfo.LocalIpAddress);
            }
        }

        public string AppRelativeCurrentExecutionFilePath => $"~{FilePath}";

        public string ApplicationPath => _request.HttpContext.RequestServices.GetRequiredService<IOptions<SystemWebAdaptersOptions>>().Value.AppDomainAppVirtualPath;

        public Uri? UrlReferrer => TypedHeaders.Referer;

        public int TotalBytes => (int)InputStream.Length;

        public bool IsAuthenticated => LogonUserIdentity?.IsAuthenticated ?? false;

        public IIdentity? LogonUserIdentity => _request.HttpContext.User.Identity;

        public Encoding? ContentEncoding => TypedHeaders.ContentType?.Encoding;

        public string? UserHostName => _request.HttpContext.Connection.RemoteIpAddress?.ToString();

        public HttpBrowserCapabilities Browser => _browser ??= new(_request.HttpContext);

        public string? this[string key] => Params[key];

        public NameValueCollection Params => _params ??= new ParamsCollection(_request);

        public byte[] BinaryRead(int count)
        {
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfNegative(count);
#else
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
#endif

            if (count == 0)
            {
                return Array.Empty<byte>();
            }

            var buffer = new byte[count];
            var read = InputStream.Read(buffer);

            if (read == 0)
            {
                return Array.Empty<byte>();
            }

            if (read < count)
            {
                Array.Resize(ref buffer, read);
            }

            return buffer;
        }

        public void SaveAs(string filename, bool includeHeaders)
        {
            using var f = new FileStream(filename, FileMode.Create);

            if (includeHeaders)
            {
                using var w = new StreamWriter(f, leaveOpen: true);

                w.Write(HttpMethod);
                w.Write(" ");
                w.Write(Path);

                // Includes the leading '?' if non-empty
                w.Write(_request.QueryString);

                w.Write(" ");
                w.WriteLine(_request.Protocol);

                foreach (var header in _request.Headers)
                {
                    w.Write(header.Key);
                    w.Write(": ");
                    w.WriteLine(header.Value);
                }

                w.WriteLine();
            }

            WriteTo(GetBufferedInputStream(), f);
        }

        /// <summary>
        /// Copies the entire stream, but ensures the position is reset to what it was when starting
        /// </summary>
        private static void WriteTo(Stream source, Stream destination)
        {
            var currentPosition = source.Position;

            source.Position = 0;
            source.CopyTo(destination);
            source.Position = currentPosition;
        }

        public void Abort() => _request.HttpContext.Abort();

        [return: NotNullIfNotNull(nameof(request))]
        public static implicit operator HttpRequest?(HttpRequestCore? request) => request?.AsSystemWeb();

        [return: NotNullIfNotNull(nameof(request))]
        public static implicit operator HttpRequestCore?(HttpRequest? request) => request?._request;

        private class StringWithQualityHeaderValueComparer : IComparer<StringWithQualityHeaderValue>
        {
            public static StringWithQualityHeaderValueComparer Instance { get; } = new();

            public int Compare(StringWithQualityHeaderValue? x, StringWithQualityHeaderValue? y)
            {
                var xValue = x?.Quality ?? 1;
                var yValue = y?.Quality ?? 1;

                return yValue.CompareTo(xValue);
            }
        }
    }
}
