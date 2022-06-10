// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

public sealed class HttpUnhandledException : HttpException
{
    public HttpUnhandledException(string message) : base(message)
    {
    }

    public HttpUnhandledException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public HttpUnhandledException()
    {
    }
}
