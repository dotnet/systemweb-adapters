// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.AspNetCore.Identity.EntityFramework6;

public class IdentityUserLoginEF6 : IdentityUserLoginEF6<string>
{
}

/// <summary>
/// Represents a login and its associated provider for a user for Entity Framework 6.
/// Extends the standard ASP.NET Core Identity user login for EF6 compatibility.
/// </summary>
/// <typeparam name="TKey">The type of the primary key of the user associated with this login.</typeparam>
public class IdentityUserLoginEF6<TKey> : IdentityUserLogin<TKey> where TKey : IEquatable<TKey>
{
}
