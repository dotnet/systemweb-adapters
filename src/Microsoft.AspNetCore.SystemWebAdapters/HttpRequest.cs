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
using System.Web.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
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

        private FeatureReference<IHttpRequestAdapterFeature> _requestFeature;

        internal HttpRequest(HttpRequestCore request)
        {
            _request = request;
            _requestFeature = FeatureReference<IHttpRequestAdapterFeature>.Default;
        }

        private IHttpRequestAdapterFeature RequestFeature => _requestFeature.Fetch(_request.HttpContext.Features) ?? throw new InvalidOperationException("Please ensure you have added the System.Web adapters middleware.");

        internal RequestHeaders TypedHeaders => _typedHeaders ??= new(_request.Headers);

        public string? Path => _request.Path.Value;

        public string? PathInfo => _request.HttpContext.Features.Get<IPathInfoFeature>()?.PathInfo ?? string.Empty;

        public string? FilePath => _request.HttpContext.Features.Get<IPathInfoFeature>()?.FileInfo ?? Path;

        public NameValueCollection Headers => _headers ??= _request.Headers.ToNameValueCollection();

        public Uri Url => new(_request.GetEncodedUrl());

        public ReadEntityBodyMode ReadEntityBodyMode => RequestFeature.Mode;

        public Stream GetBufferlessInputStream() => RequestFeature.GetBufferlessInputStream();

        public Stream GetBufferedInputStream() => RequestFeature.GetBufferedInputStream();

        [SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = Constants.ApiFromAspNet)]
        public string? RawUrl => _request.HttpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;

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
                            userLanguages[i] = qualityArray[i].Value.Value;
                        }

                        ArrayPool<StringWithQualityHeaderValue>.Shared.Return(qualityArray);

                        _userLanguages = userLanguages;
                    }
                }

                return _userLanguages;
            }
        }

        public string UserAgent => _request.Headers[HeaderNames.UserAgent];

        public string RequestType => HttpMethod;

        public NameValueCollection Form => _form ??= _request.Form.ToNameValueCollection();

        public HttpCookieCollection Cookies => _cookies ??= new(_request.Cookies);

        public HttpFileCollection Files => _files ??= new(_request.Form.Files);

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
                            _acceptTypes[i] = accept[i].MediaType.Value;
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

        public Stream InputStream => RequestFeature.InputStream;

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

        public string ApplicationPath => _request.HttpContext.RequestServices.GetRequiredService<IHttpRuntime>().AppDomainAppVirtualPath;

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
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

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

        [return: NotNullIfNotNull("request")]
        public static implicit operator HttpRequest?(HttpRequestCore? request) => request.GetAdapter();

        [return: NotNullIfNotNull("request")]
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

        /// <summary>
        /// Gets the section of a path that could be interpreted as a file path and the subsequent path.
        ///
        /// For example:
        ///
        /// Path:     /path/file.txt/subpath
        /// FilePath: /path/file.txt
        /// PathInfo: /subpath
        /// </summary>
        /// <see cref="https://learn.microsoft.com/dotnet/api/system.web.httprequest.filepath"/>
        /// <see cref="https://learn.microsoft.com/dotnet/api/system.web.httprequest.pathinfo"/>
        private void GetFileInfoPath(out StringSegment filePath, out StringSegment pathInfo)
        {
            var path = new StringSegment(Path);
            var extensionIndex = path.IndexOf('.');

            // If no extension, just return the path
            if (extensionIndex == -1)
            {
                filePath = path;
                pathInfo = string.Empty;
                return;
            }

            var endIndex = path.IndexOf('/', extensionIndex, path.Length - extensionIndex);

            // If no filepath, just return the path
            if (endIndex == -1)
            {
                filePath = path;
                pathInfo = string.Empty;
            }
            else
            {
                filePath = path.Subsegment(0, endIndex);
                pathInfo = path.Subsegment(endIndex);
            }
        }
    }
}
