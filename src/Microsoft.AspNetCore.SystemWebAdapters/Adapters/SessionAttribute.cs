// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.AspNetCore.SystemWebAdapters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SessionAttribute : Attribute
{
    public SessionBehavior Behavior { get; set; } = SessionBehavior.Preload;

    public bool IsReadOnly { get; set; }
}
