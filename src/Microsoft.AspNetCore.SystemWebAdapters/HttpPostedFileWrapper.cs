// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;

namespace System.Web;

public class HttpPostedFileWrapper : HttpPostedFileBase
{
    private readonly HttpPostedFile _file;

    public HttpPostedFileWrapper(HttpPostedFile httpPostedFile)
    {
        if (httpPostedFile == null)
        {
            throw new ArgumentNullException(nameof(httpPostedFile));
        }

        _file = httpPostedFile;
    }

    public override int ContentLength => _file.ContentLength;

    public override string ContentType => _file.ContentType;

    public override string FileName => _file.FileName;

    public override Stream InputStream => _file.InputStream;
}
