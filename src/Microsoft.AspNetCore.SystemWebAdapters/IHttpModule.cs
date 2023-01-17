// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

public interface IHttpModule
{
    void Init(HttpApplication application);

    void Dispose();
}
