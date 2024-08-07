// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Web;
using System.Web.Routing;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// A module that exposes static files from a <see cref="IFileProvider"/>
/// </summary>
internal sealed class StaticFileProviderHttpModule : IHttpModule
{
    private readonly IFileProvider _provider;

    public StaticFileProviderHttpModule(IFileProvider provider)
    {
        _provider = provider;
    }

    public void Dispose()
    {
    }

    public void Init(HttpApplication context)
    {
        context.BeginRequest += (s, e) =>
        {
            if (s is HttpApplication { Context: { } context } && _provider.GetFileInfo(context.Request.Path) is { Exists: true } info)
            {
                context.RemapHandler(new FileInfoHandler(info));
            }
        };
    }

    private sealed class FileInfoHandler : IHttpHandler
    {
        private readonly IFileInfo _info;

        public FileInfoHandler(IFileInfo info)
        {
            _info = info;
        }

        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            using var stream = _info.CreateReadStream();

            stream.CopyTo(context.Response.OutputStream);

            context.Response.ContentType = MimeMapping.GetMimeMapping(_info.Name);
            context.Response.StatusCode = 200;
        }
    }
}
