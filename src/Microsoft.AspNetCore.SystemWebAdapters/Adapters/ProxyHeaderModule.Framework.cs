// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class ProxyHeaderModule : IHttpModule
{
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
            context.BeginRequest += static (s, e) => UseHeaders(((HttpApplication)s).Context.Request);
        }
        else
        {
            if (_options.ServerName is null)
            {
                throw new InvalidOperationException("Server name must be set for proxy options.");
            }

            context.BeginRequest += (s, e) => UseOptions(((HttpApplication)s).Context.Request);
        }

        context.PreSendRequestHeaders += (s, e) => FixupLocationHeader(((HttpApplication)s).Context);
    }

    private static void FixupLocationHeader(HttpContext context)
    {
        // Some scenarios (such as OIDC auth) will store a URL (including the local host/port rather than the forwarded one)
        // and later redirect users to that URL. If a response contains a Location header, ensure that the correct scheme,
        // host, and port are used.
        if (context.Response.Headers["Location"] is { } location)
        {
            var host = context.Request.ServerVariables.Get("SERVER_NAME");
            var scheme = context.Request.ServerVariables.Get("SERVER_PROTOCOL");
            var port = GetPort(context.Request.ServerVariables.Get("SERVER_PORT"), scheme);
            var originalHost = context.Request.ServerVariables.Get("ORIGINAL_SERVER_NAME");
            var originalScheme = context.Request.ServerVariables.Get("ORIGINAL_SERVER_PROTOCOL");
            var originalPort = GetPort(context.Request.ServerVariables.Get("ORIGINAL_SERVER_PORT"), originalScheme);

            if (Uri.TryCreate(location, UriKind.RelativeOrAbsolute, out var initialLocationUri) && initialLocationUri.IsAbsoluteUri)
            {
                var locationUri = new UriBuilder(location);

                if (locationUri.Host.Equals(originalHost, StringComparison.OrdinalIgnoreCase) && locationUri.Port == originalPort)
                {
                    // If the host and port for the redirect location match the original host/port that was replaced
                    // with proxy server data, then update the redirect location.
                    locationUri.Host = host;
                    if (port.HasValue)
                    {
                        locationUri.Port = port.Value;
                    }
                    if (scheme is not null)
                    {
                        locationUri.Scheme = scheme;
                    }

                    context.Response.Headers["Location"] = locationUri.ToString();
                }
            }
        }
    }

    private void UseOptions(HttpRequest request)
    {
        UseForwardedFor(request);

        StoreOriginalServerVariables(request);
        request.ServerVariables.Set("SERVER_NAME", _options.ServerName);
        request.ServerVariables.Set("SERVER_PORT", _options.ServerPortString);
        request.ServerVariables.Set("SERVER_PROTOCOL", _options.Scheme);
    }

    private static void UseHeaders(HttpRequest request)
    {
        UseForwardedFor(request);

        if (request.Headers["x-forwarded-host"] is { } host)
        {
            StoreOriginalServerVariables(request);

            var value = new ForwardedHost(host);

            request.ServerVariables.Set("SERVER_NAME", value.ServerName);

            if (value.Port is { })
            {
                request.ServerVariables.Set("SERVER_PORT", value.Port);
            }

            if (request.Headers["x-forwarded-proto"] is { } proto)
            {
                request.ServerVariables.Set("SERVER_PROTOCOL", proto);
            }
        }
    }

    private static void UseForwardedFor(HttpRequest request)
    {
        if (request.Headers["x-forwarded-for"] is { } remote)
        {
            request.ServerVariables.Set("REMOTE_ADDR", remote);
            request.ServerVariables.Set("REMOTE_HOST", remote);
        }
    }

    private static void StoreOriginalServerVariables(HttpRequest request)
    {
        request.ServerVariables.Set("ORIGINAL_SERVER_NAME", request.ServerVariables["SERVER_NAME"]);
        request.ServerVariables.Set("ORIGINAL_SERVER_PORT", request.ServerVariables["SERVER_PORT"]);
        request.ServerVariables.Set("ORIGINAL_SERVER_PROTOCOL", request.ServerVariables["SERVER_PROTOCOL"]);
    }

    private static int? GetPort(string? portString, string protocol)
    {
        if (int.TryParse(portString, out var port))
        {
            return port;
        }

        if ("HTTPS".Equals(protocol, StringComparison.OrdinalIgnoreCase))
        {
            return 443;
        }

        if ("HTTP".Equals(protocol, StringComparison.OrdinalIgnoreCase))
        {
            return 80;
        }

        return null;
    }
}
