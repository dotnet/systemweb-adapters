// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

public interface IHttpHandlerFactory
{
    [Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = Constants.ApiFromAspNet)]
    IHttpHandler GetHandler(HttpContext context, String requestType, String url, String pathTranslated);

    void ReleaseHandler(IHttpHandler handler);
}
