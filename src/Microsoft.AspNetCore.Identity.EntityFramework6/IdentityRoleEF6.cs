// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Identity.EntityFramework6;

/// <summary>
/// The default implementation of <see cref="IdentityRoleEF6{TKey}"/> which uses a string as a primary key.
/// </summary>
public class IdentityRoleEF6 : IdentityRoleEF6<string, IdentityUserRoleEF6>
{
    /// <summary>
    /// Initializes a new instance of <see cref="IdentityRoleEF6"/>.
    /// </summary>
    /// <remarks>
    /// The Id property is initialized to form a new GUID string value.
    /// </remarks>
    public IdentityRoleEF6()
    {
        Id = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="IdentityRoleEF6"/>.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <remarks>
    /// The Id property is initialized to form a new GUID string value.
    /// </remarks>
    public IdentityRoleEF6(string roleName) : this()
    {
        Name = roleName;
    }
}

/// <summary>
/// Represents a role in the identity system for Entity Framework 6.
/// This class extends Identity Core's base role with navigation properties for EF6 compatibility.
/// </summary>
/// <typeparam name="TKey">The type of the primary key for the role.</typeparam>
/// <typeparam name="TUserRole">The type representing a user role.</typeparam>
public class IdentityRoleEF6<TKey, TUserRole> : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserRole : IdentityUserRoleEF6<TKey>
{
    /// <summary>
    ///     Constructor
    /// </summary>
    public IdentityRoleEF6()
    {
        Users = new List<TUserRole>();
    }

    public virtual ICollection<TUserRole> Users { get; private set; }
}
