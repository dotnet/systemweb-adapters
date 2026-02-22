// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.AspNetCore.Identity.EntityFramework6;

/// <summary>
/// Represents an authentication token for a user for Entity Framework 6.
/// Extends the standard ASP.NET Core Identity user token for EF6 compatibility.
/// </summary>
/// <typeparam name="TKey">The type of the primary key used for users.</typeparam>
public class IdentityUserTokenEF6<TKey> : IdentityUserToken<TKey> where TKey : IEquatable<TKey>
{
}
