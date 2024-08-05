// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using Microsoft.AspNetCore.Http;

namespace System.Web;

public class HttpException : SystemException
{
    private readonly int _httpStatusCode; 

    public HttpException()
    {
    }

    public HttpException(string message) : base(message)
    {
    }

    public HttpException(String message, Exception innerException)
        : base(message, innerException)
    {
    }

    public HttpException(int httpStatusCode, string message)
        : base(message)
    {
        _httpStatusCode = httpStatusCode;
    }

    public HttpException(int httpStatusCode, string message, Exception innerException)
        : base(message, innerException)
    {
        _httpStatusCode = httpStatusCode;
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = Constants.ApiFromAspNet)]
    public int GetHttpCode() => GetHttpCodeForException(this);

    internal static int GetHttpCodeForException(Exception e) => e switch
    {
        HttpException { _httpStatusCode: { } code } when code > 0 => code,
        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
        PathTooLongException => StatusCodes.Status414UriTooLong,
        { InnerException: { } inner } => GetHttpCodeForException(inner),
        _ => 500
    };
}
