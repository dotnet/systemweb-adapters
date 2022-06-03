﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

public class SessionSerializerOptions
{
    /// <summary>
    /// Gets or sets whether an exception should be thrown if an unknown session key is encountered.
    /// </summary>
    public bool ThrowOnUnknownSessionKey { get; set; } = true;
}
