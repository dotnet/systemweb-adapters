// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class HttpHandlerEndpointFeature :
    IHttpHandlerFeature,
    IEndpointFeature,
    IHttpRequestPathFeature,
    IHttpRequestFeature
{
    private readonly HttpContextCore _context;
    private readonly IHttpRequestFeature _request;

    private Container _current;
    private Container _previous;

    private string? _pathInfo;
    private string? _filePath;

    public HttpHandlerEndpointFeature(HttpContextCore context, IHttpRequestFeature request, IEndpointFeature? existingEndpoint)
    {
        _context = context;
        _request = request;
        _current = new(_context, endpoint: existingEndpoint?.Endpoint);
    }

    Endpoint? IEndpointFeature.Endpoint
    {
        get => _current.Endpoint;
        set
        {
            _previous = _current;
            _current = new(_context, endpoint: value);
            ClearPath();
        }
    }

    IHttpHandler? IHttpHandlerFeature.Current
    {
        get => _current.Handler;
        set
        {
            _previous = _current;
            _current = new(_context, handler: value);
            ClearPath();
        }
    }

    private void ClearPath()
    {
        _filePath = null;
        _pathInfo = null;
    }

    [MemberNotNull(nameof(_filePath))]
    [MemberNotNull(nameof(_pathInfo))]
    private void EnsurePath()
    {
        if (_filePath is null || _pathInfo is null)
        {
            if (_current.Endpoint?.Metadata.GetMetadata<HttpHandlerRouteMetadata>() is { } metadata)
            {
                (_filePath, _pathInfo) = metadata.GetInfo(_request.Path);
                return;
            }
        }

        _filePath = _request.Path;
        _pathInfo = string.Empty;
    }

    IHttpHandler? IHttpHandlerFeature.Previous => _previous.Handler;

    string IHttpRequestPathFeature.Path => _request.Path;

    string IHttpRequestPathFeature.RawUrl => _request.RawTarget;

    string IHttpRequestPathFeature.PathInfo
    {
        get
        {
            if (_pathInfo is null)
            {
                EnsurePath();
            }

            return _pathInfo;
        }
    }

    string IHttpRequestPathFeature.FilePath
    {
        get
        {
            if (_filePath is null)
            {
                EnsurePath();
            }

            return _filePath;
        }
    }

    string IHttpRequestFeature.Protocol
    {
        get => _request.Protocol;
        set => _request.Protocol = value;
    }

    string IHttpRequestFeature.Scheme
    {
        get => _request.Scheme;
        set => _request.Scheme = value;
    }

    string IHttpRequestFeature.Method
    {
        get => _request.Method;
        set => _request.Method = value;
    }

    string IHttpRequestFeature.PathBase
    {
        get => _request.PathBase;
        set
        {
            ClearPath();
            _request.PathBase = value;
        }
    }

    string IHttpRequestFeature.Path
    {
        get => _request.Path;
        set
        {
            ClearPath();
            _request.Path = value;
        }
    }

    string IHttpRequestFeature.QueryString
    {
        get => _request.QueryString;
        set => _request.QueryString = value;
    }

    string IHttpRequestFeature.RawTarget
    {
        get => _request.RawTarget;
        set => _request.RawTarget = value;
    }

    IHeaderDictionary IHttpRequestFeature.Headers
    {
        get => _request.Headers;
        set => _request.Headers = value;
    }

    Stream IHttpRequestFeature.Body
    {
        get => _request.Body;
        set => _request.Body = value;
    }

    private struct Container
    {
        private readonly HttpContextCore _context;

        private Endpoint? _endpoint;
        private IHttpHandler? _handler;

        public Container(HttpContextCore context, Endpoint? endpoint = null, IHttpHandler? handler = null)
        {
            _context = context;
            _endpoint = endpoint;
            _handler = handler;
        }

        public Endpoint? Endpoint
        {
            get
            {
                if (_endpoint is null)
                {
                    if (_handler is null)
                    {
                        return null;
                    }

                    _endpoint = _context.CreateEndpoint(_handler);
                }

                return _endpoint;
            }
        }

        public IHttpHandler? Handler
        {
            get
            {
                if (_handler is null)
                {
                    if (_endpoint is null)
                    {
                        return null;
                    }

                    _handler = _context.CreateHandler(_endpoint);
                }

                return _handler;
            }
        }
    }
}

