// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.AspNetCore.Identity.EntityFramework6;

/// <summary>
/// Represents a claim that is granted to all users within a role for Entity Framework 6.
/// Extends the standard ASP.NET Core Identity role claim for EF6 compatibility.
/// </summary>
/// <typeparam name="TKey">The type of the primary key of the role associated with this claim.</typeparam>
public class IdentityRoleClaimEF6<TKey> : IdentityRoleClaim<TKey> where TKey : IEquatable<TKey>
{
}
