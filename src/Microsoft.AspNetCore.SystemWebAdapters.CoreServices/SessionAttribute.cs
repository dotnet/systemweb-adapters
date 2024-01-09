// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web.SessionState;

namespace Microsoft.AspNetCore.SystemWebAdapters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SessionAttribute : Attribute
{
    [Obsolete("Prefer SessionBehavior instead.")]
    public SessionBehavior Behavior
    {
        get
        {
            if (SessionBehavior is SessionStateBehavior.Disabled)
            {
                return SystemWebAdapters.SessionBehavior.None;
            }

            return IsPreLoad ? SystemWebAdapters.SessionBehavior.Preload : SystemWebAdapters.SessionBehavior.OnDemand;
        }
        set
        {
            SessionBehavior = value switch
            {
                SystemWebAdapters.SessionBehavior.None => SessionStateBehavior.Disabled,
                SystemWebAdapters.SessionBehavior.Preload => SessionStateBehavior.Required,
                SystemWebAdapters.SessionBehavior.OnDemand => SessionStateBehavior.Required,
                _ => throw new ArgumentOutOfRangeException(nameof(value)),
            };
        }
    }

    public SessionStateBehavior SessionBehavior { get; set; } = SessionStateBehavior.Required;

    public bool IsPreLoad { get; set; } = true;

    public bool IsReadOnly
    {
        get => SessionBehavior is SessionStateBehavior.ReadOnly;
        [Obsolete("Prefer SessionBehavior property")]
        set
        {
            if (value)
            {
                SessionBehavior = SessionStateBehavior.ReadOnly;
            }
        }
    }
}
