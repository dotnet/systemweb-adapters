// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public interface IRemoteAuthenticateResultProcessor
{
    Task ProcessAsync(RemoteAuthenticationResult result, HttpContext context);
}
