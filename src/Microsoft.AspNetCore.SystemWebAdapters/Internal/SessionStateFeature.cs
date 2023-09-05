// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState;

namespace Microsoft.AspNetCore.SystemWebAdapters.Internal
{
    internal class SessionStateFeature: ISessionStateFeature
    {
        public SessionStateBehavior State { get; set; }
    }
}
