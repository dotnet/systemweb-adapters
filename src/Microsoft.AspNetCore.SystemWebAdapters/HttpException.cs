// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net;

namespace System.Web;

public class HttpException : SystemException
{
    private readonly int _httpStatusCode = 500;
    
    public HttpException()
    {
    }

    public HttpException(string message) : base(message)
    {
    }

    public HttpException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public HttpException(int httpStatusCode)
    {
        _httpStatusCode = httpStatusCode;
    }

    public HttpException(HttpStatusCode httpStatusCode)
    {
        _httpStatusCode = (int)httpStatusCode;
    }

    public HttpException(int httpStatusCode, string message)
        : base(message)
    {
        _httpStatusCode = httpStatusCode;
    }

    public HttpException(HttpStatusCode httpStatusCode, string message)
        : base(message)
    {
        _httpStatusCode = (int)httpStatusCode;
    }

    public HttpException(int httpStatusCode, string message, Exception innerException)
        : base(message, innerException)
    {
        _httpStatusCode = httpStatusCode;
    }

    public HttpException(HttpStatusCode httpStatusCode, string message, Exception innerException)
        : base(message, innerException)
    {
        _httpStatusCode = (int)httpStatusCode;
    }

    public int GetHttpCode()
    {
        return _httpStatusCode;
    }

    public int StatusCode
    {
        get
        {
            return _httpStatusCode;
        }
    }
}
