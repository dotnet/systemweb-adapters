// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.AspNetCore.Identity.EntityFramework6;

public class IdentityUserRoleEF6 : IdentityUserRoleEF6<string>
{
}

/// <summary>
/// Represents the link between a user and a role.
/// Extends the standard ASP.NET Core Identity user role for EF6.
/// </summary>
/// <typeparam name="TKey">The type of the primary key used for users and roles.</typeparam>
public class IdentityUserRoleEF6<TKey> : IdentityUserRole<TKey> where TKey : IEquatable<TKey>
{
}
