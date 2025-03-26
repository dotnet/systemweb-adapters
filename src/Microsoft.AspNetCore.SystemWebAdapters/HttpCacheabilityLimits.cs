// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

enum HttpCacheabilityLimits
{
    MinValue = HttpCacheability.NoCache,
    MaxValue = HttpCacheability.ServerAndPrivate,
    None = MaxValue + 1,
}
