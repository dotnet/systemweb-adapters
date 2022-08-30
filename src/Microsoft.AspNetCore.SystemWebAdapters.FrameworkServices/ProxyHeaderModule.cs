// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using System.Web;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// Updates server and request variables based on proxy headers. See https://docs.microsoft.com/en-us/iis/web-dev-reference/server-variables for reference on what server variables should be used.
/// </summary>
internal class ProxyHeaderModule : IHttpModule
{
    private const string Host = "Host";
    private const string ServerHttps = "HTTPS";
    private const string ServerName = "SERVER_NAME";
    private const string ServerPort = "SERVER_PORT";
    private const string ForwardedProto = "x-forwarded-proto";
    private const string ForwardedHost = "x-forwarded-host";
    
    // ASP.NET expects lowercase values for HTTPS, not uppercase as the docs may indicate
    private const string On = "on";
    private const string Off = "off";

    private readonly IOptions<ProxyOptions> _options;
    private readonly ReflectionHelper _reflectionHelper;

    public ProxyHeaderModule(IOptions<ProxyOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _reflectionHelper = new ReflectionHelper();
    }

    public void Dispose()
    {
    }

    public void Init(HttpApplication context)
    {
        var options = _options.Value;

        if (options.UseForwardedHeaders)
        {
            context.BeginRequest += (s, e) =>
            {
                var request = ((HttpApplication)s).Context.Request;
                UseHeaders(request.Headers, request.ServerVariables, request);
            };
        }
        else
        {
            var values = new ServerValues(options);

            context.BeginRequest += (s, e) =>
            {
                var request = ((HttpApplication)s).Context.Request;
                UseOptions(values, request.Headers, request.ServerVariables, request);
            };
        }
    }

    public void UseHeaders(NameValueCollection requestHeaders, NameValueCollection serverVariables, HttpRequest? request = null)
    {
        UseForwardedFor(requestHeaders, serverVariables);

        var proto = requestHeaders[ForwardedProto];

        if (requestHeaders[ForwardedHost] is { } host)
        {
            if (requestHeaders[Host] is { } originalHost)
            {
                requestHeaders[_options.Value.OriginalHostHeaderName] = originalHost;
            }

            var value = new ForwardedHost(host, proto);

            serverVariables.Set(ServerName, value.ServerName);
            serverVariables.Set(ServerPort, value.Port.ToString(CultureInfo.InvariantCulture));
            serverVariables.Set(ServerHttps, value.IsSecure ? On : Off);

            requestHeaders[Host] = host;

            if (request != null)
            {
                _reflectionHelper.SetHostHeader(request, host);
            }
        }
    }

    private void UseOptions(ServerValues values, NameValueCollection requestHeaders, NameValueCollection serverVariables, HttpRequest? request = null)
    {
        UseForwardedFor(requestHeaders, serverVariables);

        serverVariables.Set(ServerName, values.Name);
        serverVariables.Set(ServerPort, values.Port);
        serverVariables.Set(ServerHttps, values.Https);
        requestHeaders[Host] = values.Host;

        if (request != null)
        {
            _reflectionHelper.SetHostHeader(request, values.Host);
        }
    }

    private static void UseForwardedFor(NameValueCollection requestHeaders, NameValueCollection serverVariables)
    {
        if (requestHeaders["x-forwarded-for"] is { } remote)
        {
            serverVariables.Set("REMOTE_ADDR", remote);
            serverVariables.Set("REMOTE_HOST", remote);
        }
    }

    private class ReflectionHelper
    {
        private readonly FieldInfo _wrField;
        private readonly FieldInfo _knownRequestHeadersField;

        public ReflectionHelper()
        {
            _wrField = typeof(HttpRequest).GetField("_wr", BindingFlags.NonPublic | BindingFlags.Instance);
            var type = Assembly.GetAssembly(typeof(HttpRequest)).GetType("System.Web.Hosting.IIS7WorkerRequest");
            _knownRequestHeadersField = type.GetField("_knownRequestHeaders", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        
        public void SetHostHeader(HttpRequest request, string host)
        {
            // Need to use reflection to force HttpRequest to update the known headers
            var workerRequest = _wrField.GetValue(request);
            var knownRequestHeaders = (string[])_knownRequestHeadersField.GetValue(workerRequest);
            // We don't need to do anything if the known headers haven't been built yet
            if (knownRequestHeaders != null)
            {
                knownRequestHeaders[HttpWorkerRequest.HeaderHost] = host;
            }
        }
    }

    private class ServerValues
    {
        public ServerValues(ProxyOptions options)
        {
            if (options.ServerName is null)
            {
                throw new InvalidOperationException("Server name must be set for proxy options.");
            }

            Name = options.ServerName;
            Port = options.ServerPort.ToString(CultureInfo.InvariantCulture);
            Https = string.Equals("https", options.Scheme, StringComparison.OrdinalIgnoreCase) ? On : Off;
            Host = $"{Name}:{Port}";
        }

        public string Name { get; }

        public string Port { get; }

        public string Https { get; }

        public string Host { get; }
    }
}
