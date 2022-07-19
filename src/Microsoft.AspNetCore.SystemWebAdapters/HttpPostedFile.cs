// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using Microsoft.AspNetCore.Http;

namespace System.Web;

public sealed class HttpPostedFile
{
    internal HttpPostedFile(IFormFile file) => File = file;

    internal IFormFile File { get; }

    public string FileName => File.FileName;

    public string ContentType => File.ContentType;

    public int ContentLength => (int)File.Length;

    public Stream InputStream => File.OpenReadStream();
}
