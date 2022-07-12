// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class ProxyHeaderModule : IHttpModule
{
    private const string Host = "Host";
    private const string ServerName = "SERVER_NAME";
    private const string ServerPort = "SERVER_PORT";
    private const string ServerProtocol = "SERVER_PROTOCOL";
    private const string ForwardedProto = "x-forwarded-proto";
    private const string ForwardedHost = "x-forwarded-host";

    private readonly ProxyOptions _options;

    public ProxyHeaderModule(ProxyOptions options)
    {
        _options = options;
    }

    public void Dispose()
    {
    }

    public void Init(HttpApplication context)
    {
        if (_options.UseForwardedHeaders)
        {
            context.BeginRequest += (s, e) =>
            {
                var request = ((HttpApplication)s).Context.Request;
                UseHeaders(request.Headers, request.ServerVariables);
            };
        }
        else
        {
            if (_options.ServerName is null)
            {
                throw new InvalidOperationException("Server name must be set for proxy options.");
            }

            context.BeginRequest += (s, e) =>
            {
                var request = ((HttpApplication)s).Context.Request;
                UseOptions(request.Headers, request.ServerVariables);
            };
        }
    }

    public void UseHeaders(NameValueCollection requestHeaders, NameValueCollection serverVariables)
    {
        UseForwardedFor(requestHeaders, serverVariables);

        var proto = requestHeaders[ForwardedProto];

        if (requestHeaders[ForwardedHost] is { } host)
        {
            if (requestHeaders[Host] is { } originalHost)
            {
                requestHeaders[_options.OriginalHostHeaderName] = originalHost;
            }

            var value = new ForwardedHost(host, proto);

            serverVariables.Set(ServerName, value.ServerName);
            serverVariables.Set(ServerPort, value.Port.ToString(CultureInfo.InvariantCulture));

            requestHeaders[Host] = host;
        }

        if (proto is { })
        {
            serverVariables.Set(ServerProtocol, proto);
        }
    }

    private void UseOptions(NameValueCollection requestHeaders, NameValueCollection serverVariables)
    {
        UseForwardedFor(requestHeaders, serverVariables);

        serverVariables.Set(ServerName, _options.ServerName);
        serverVariables.Set(ServerPort, _options.ServerPortString);
        serverVariables.Set(ServerProtocol, _options.Scheme);
        requestHeaders[Host] = _options.ServerHostString;
    }

    private static void UseForwardedFor(NameValueCollection requestHeaders, NameValueCollection serverVariables)
    {
        if (requestHeaders["x-forwarded-for"] is { } remote)
        {
            serverVariables.Set("REMOTE_ADDR", remote);
            serverVariables.Set("REMOTE_HOST", remote);
        }
    }
}
