// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

internal class ForwardedClaimsPrincipalModule : IHttpModule
{
    public void Init(HttpApplication context)
    {
        context.AuthenticateRequest += AuthenticateWithForwardedClaimsPrincipal;
    }

    private void AuthenticateWithForwardedClaimsPrincipal(object sender, EventArgs e)
    {
        var context = ((HttpApplication)sender).Context;

        // Get the serialized claims principal if possible
        var serializedClaimsPrincipal = context.Request.ServerVariables[Constants.IdentityVariableName];
        if (serializedClaimsPrincipal is null)
        {
            // If the principal isn't set as a server variable, check headers
            serializedClaimsPrincipal = context.Request.Headers[Constants.IdentityVariableName];
        }

        // If a claims principal was found, deserialize and set it
        if (serializedClaimsPrincipal is not null)
        {
            // Deserialize the claims principal from server variables, if available
            var claimsPrincipalBytes = Convert.FromBase64String(serializedClaimsPrincipal);
            using var reader = new BinaryReader(new MemoryStream(claimsPrincipalBytes));
            var claimsPrincipal = new ClaimsPrincipal(reader);

            // Set the claims principal
            context.User = claimsPrincipal;

            // Make sure the Principal's are in sync
            // http://www.hanselman.com/blog/systemthreadingthreadcurrentprincipal-vs-systemwebhttpcontextcurrentuser-or-why-formsauthentication-can-be-subtle
            Thread.CurrentPrincipal = context.User;
        }
    }

    public void Dispose()
    {
    }
}
