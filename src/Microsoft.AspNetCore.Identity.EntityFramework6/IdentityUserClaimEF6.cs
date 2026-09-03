// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.AspNetCore.Identity.EntityFramework6;

public class IdentityUserClaimEF6 : IdentityUserClaimEF6<string>
{
}

/// <summary>
/// Represents a claim that a user possesses for Entity Framework 6.
/// Extends the standard ASP.NET Core Identity user claim for EF6 compatibility.
/// </summary>
/// <typeparam name="TKey">The type used for the primary key for this user that possesses this claim.</typeparam>
public class IdentityUserClaimEF6<TKey> : IdentityUserClaim<TKey> where TKey : IEquatable<TKey>
{
}
