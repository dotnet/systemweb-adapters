// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web.SessionState;

namespace Microsoft.AspNetCore.SystemWebAdapters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SessionAttribute : Attribute
{
    public SessionStateBehavior SessionBehavior { get; set; } = SessionStateBehavior.Required;

    public bool IsPreLoad { get; set; } = true;

    public bool IsReadOnly => SessionBehavior is SessionStateBehavior.ReadOnly;
}
