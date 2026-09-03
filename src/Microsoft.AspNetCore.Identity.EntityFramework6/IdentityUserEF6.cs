// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.AspNetCore.Identity.EntityFramework6;

/// <summary>
/// The default implementation of <see cref="IdentityUser{TKey}"/> which uses a string as the primary key.
/// </summary>
public class IdentityUserEF6 : IdentityUserEF6<string, IdentityUserLoginEF6, IdentityUserRoleEF6, IdentityUserClaimEF6>
{
    /// <summary>
    /// Initializes a new instance of <see cref="IdentityUserEF6"/>.
    /// </summary>
    /// <remarks>
    /// The Id property is initialized to form a new GUID string value.
    /// </remarks>
    public IdentityUserEF6()
    {
        Id = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="IdentityUserEF6"/>.
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <remarks>
    /// The Id property is initialized to form a new GUID string value.
    /// </remarks>
    public IdentityUserEF6(string userName) : this()
    {
        UserName = userName;
    }
}

/// <summary>
/// Represents a user in the identity system for Entity Framework 6.
/// This class extends Identity Core's base user with navigation properties for EF6 compatibility.
/// </summary>
/// <typeparam name="TKey">The type used for the primary key for the user.</typeparam>
/// <typeparam name="TLogin">The type representing a user login.</typeparam>
/// <typeparam name="TRole">The type representing a user role.</typeparam>
/// <typeparam name="TClaim">The type representing a user claim.</typeparam>
public class IdentityUserEF6<TKey, TLogin, TRole, TClaim> : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
    where TLogin : IdentityUserLoginEF6<TKey>
    where TRole : IdentityUserRoleEF6<TKey>
    where TClaim : IdentityUserClaimEF6<TKey>
{
    public IdentityUserEF6()
    {
        Claims = new List<TClaim>();
        Roles = new List<TRole>();
        Logins = new List<TLogin>();
    }

    /// <summary>
    ///     DateTime in UTC when lockout ends, any time in the past is considered not locked out.
    /// </summary>
    public virtual DateTime? LockoutEndDateUtc { get; set; }

    [NotMapped]
    public override DateTimeOffset? LockoutEnd
    {
        get => LockoutEndDateUtc;
        set => LockoutEndDateUtc = value switch
        {
            { } v => v.UtcDateTime,
            _ => default,
        };
    }

    /// <summary>
    ///     Navigation property for user roles
    /// </summary>
    public virtual ICollection<TRole> Roles { get; private set; }

    /// <summary>
    ///     Navigation property for user claims
    /// </summary>
    public virtual ICollection<TClaim> Claims { get; private set; }

    /// <summary>
    ///     Navigation property for user logins
    /// </summary>
    public virtual ICollection<TLogin> Logins { get; private set; }
}
