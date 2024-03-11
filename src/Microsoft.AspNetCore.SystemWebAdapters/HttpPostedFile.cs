// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
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

    public void SaveAs(string filename)
    {
        if (!Path.IsPathRooted(filename))
        {
            throw new HttpException(string.Format(CultureInfo.InvariantCulture,
                "The SaveAs method is configured to require a rooted path, and the path '{0}' is not rooted", filename));
        }

        using (var fileStream = new FileStream(filename, FileMode.Create))
        {
            InputStream.CopyTo(fileStream);
        }
    }
}
