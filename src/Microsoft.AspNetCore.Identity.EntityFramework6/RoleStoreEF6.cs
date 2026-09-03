// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Microsoft.AspNetCore.Identity.EntityFramework6;

/// <summary>
/// Creates a new instance of a persistence store for the specified role type.
/// </summary>
/// <typeparam name="TRole">The type of the class representing a role.</typeparam>
public class RoleStoreEF6<TRole> : RoleStoreEF6<TRole, IdentityEF6DbContext, string>
    where TRole : IdentityRoleEF6<string, IdentityUserRoleEF6<string>>, new()
{
    /// <summary>
    /// Constructs a new instance of <see cref="RoleStoreEF6{TRole}"/>.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/>.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
    public RoleStoreEF6(IdentityEF6DbContext context, IdentityErrorDescriber? describer = null) : base(context, describer)
    {
    }
}

/// <summary>
/// Creates a new instance of a persistence store for roles.
/// </summary>
/// <typeparam name="TRole">The type of the class representing a role.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
public class RoleStoreEF6<TRole, TContext> : RoleStoreEF6<TRole, TContext, string>
    where TRole : IdentityRoleEF6<string, IdentityUserRoleEF6<string>>, new()
    where TContext : DbContext
{
    /// <summary>
    /// Constructs a new instance of <see cref="RoleStoreEF6{TRole, TContext}"/>.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/>.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
    public RoleStoreEF6(TContext context, IdentityErrorDescriber? describer = null) : base(context, describer)
    {
    }
}

/// <summary>
/// Creates a new instance of a persistence store for roles.
/// </summary>
/// <typeparam name="TRole">The type of the class representing a role.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
/// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
public class RoleStoreEF6<TRole, TContext, TKey> : RoleStoreEF6<TRole, TContext, TKey, IdentityUserRoleEF6<TKey>, IdentityRoleClaimEF6<TKey>>
    where TRole : IdentityRoleEF6<TKey, IdentityUserRoleEF6<TKey>>, new()
    where TContext : DbContext
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Constructs a new instance of <see cref="RoleStoreEF6{TRole, TContext, TKey}"/>.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/>.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
    public RoleStoreEF6(TContext context, IdentityErrorDescriber? describer = null) : base(context, describer)
    {
    }
}

/// <summary>
/// Creates a new instance of a persistence store for roles.
/// </summary>
/// <typeparam name="TRole">The type of the class representing a role.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
/// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
/// <typeparam name="TUserRole">The type representing a user role.</typeparam>
/// <typeparam name="TRoleClaim">The type representing a role claim.</typeparam>
public class RoleStoreEF6<TRole, TContext, TKey, TUserRole, TRoleClaim> :
    RoleStoreBase<TRole, TKey, TUserRole, TRoleClaim>,
    IQueryableRoleStore<TRole>,
    IRoleClaimStore<TRole>
    where TRole : IdentityRoleEF6<TKey, TUserRole>
    where TKey : IEquatable<TKey>
    where TContext : DbContext
    where TUserRole : IdentityUserRoleEF6<TKey>, new()
    where TRoleClaim : IdentityRoleClaimEF6<TKey>, new()
{
    /// <summary>
    /// Constructs a new instance of <see cref="RoleStoreEF6{TRole, TContext, TKey, TUserRole, TRoleClaim}"/>.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/>.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
    public RoleStoreEF6(TContext context, IdentityErrorDescriber? describer = null) : base(describer ?? new IdentityErrorDescriber())
    {
        ArgumentNullException.ThrowIfNull(context);
        Context = context;
    }

    /// <summary>
    /// Gets the database context for this store.
    /// </summary>
    public TContext Context { get; private set; }

    /// <summary>
    /// A navigation property for the roles the store contains.
    /// </summary>
    public override IQueryable<TRole> Roles => Context.Set<TRole>();

    /// <summary>
    /// Gets or sets a flag indicating if changes should be persisted after CreateAsync, UpdateAsync and DeleteAsync are called.
    /// </summary>
    /// <value>
    /// True if changes should be automatically persisted, otherwise false.
    /// </value>
    public bool AutoSaveChanges { get; set; } = true;

    /// <summary>Saves the current store.</summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    protected Task SaveChanges(CancellationToken cancellationToken)
    {
        return AutoSaveChanges ? Context.SaveChangesAsync() : Task.CompletedTask;
    }

    /// <summary>
    /// Creates a new role in a store as an asynchronous operation.
    /// </summary>
    /// <param name="role">The role to create in the store.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the <see cref="IdentityResult"/> of the asynchronous query.</returns>
    public override async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);
        Context.Set<TRole>().Add(role);
        await SaveChanges(cancellationToken);
        return IdentityResult.Success;
    }

    /// <summary>
    /// Updates a role in a store as an asynchronous operation.
    /// </summary>
    /// <param name="role">The role to update in the store.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the <see cref="IdentityResult"/> of the asynchronous query.</returns>
    public override async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);
        
        var entry = Context.Entry(role);
        if (entry.State == EntityState.Detached)
        {
            Context.Set<TRole>().Attach(role);
            entry.State = EntityState.Modified;
        }
        
        role.ConcurrencyStamp = Guid.NewGuid().ToString();
        
        try
        {
            await SaveChanges(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }
        return IdentityResult.Success;
    }

    /// <summary>
    /// Deletes a role from the store as an asynchronous operation.
    /// </summary>
    /// <param name="role">The role to delete from the store.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the <see cref="IdentityResult"/> of the asynchronous query.</returns>
    public override async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);
        Context.Set<TRole>().Remove(role);
        try
        {
            await SaveChanges(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }
        return IdentityResult.Success;
    }

    /// <summary>
    /// Finds the role who has the specified ID as an asynchronous operation.
    /// </summary>
    /// <param name="id">The role ID to look for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that result of the look up.</returns>
    public override Task<TRole?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        var roleId = ConvertIdFromString(id);
        return Roles.FirstOrDefaultAsync(r => r.Id!.Equals(roleId), cancellationToken);
    }

    /// <summary>
    /// Finds the role who has the specified normalized name as an asynchronous operation.
    /// </summary>
    /// <param name="normalizedName">The normalized role name to look for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that result of the look up.</returns>
    public override Task<TRole?> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        // Query against Name with normalization in the query since NormalizedName is not mapped
        return Roles.FirstOrDefaultAsync(r => r.Name.ToUpper() == normalizedName, cancellationToken);
    }

    /// <summary>
    /// Get the claims associated with the specified <paramref name="role"/> as an asynchronous operation.
    /// </summary>
    /// <param name="role">The role whose claims should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the claims granted to a role.</returns>
    public override async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);

        var roleClaims = await Context.Set<TRoleClaim>()
            .Where(rc => rc.RoleId.Equals(role.Id))
            .ToListAsync(cancellationToken);

        return roleClaims
            .Select(c => new Claim(c.ClaimType!, c.ClaimValue!))
            .ToList();
    }

    /// <summary>
    /// Adds the <paramref name="claim"/> given to the specified <paramref name="role"/>.
    /// </summary>
    /// <param name="role">The role to add the claim to.</param>
    /// <param name="claim">The claim to add to the role.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public override Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);
        ArgumentNullException.ThrowIfNull(claim);
        Context.Set<TRoleClaim>().Add(CreateRoleClaim(role, claim));
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes the <paramref name="claim"/> given from the specified <paramref name="role"/>.
    /// </summary>
    /// <param name="role">The role to remove the claim from.</param>
    /// <param name="claim">The claim to remove from the role.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public override async Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);
        ArgumentNullException.ThrowIfNull(claim);
        var claims = await Context.Set<TRoleClaim>()
            .Where(rc => rc.RoleId.Equals(role.Id) && rc.ClaimValue == claim.Value && rc.ClaimType == claim.Type)
            .ToListAsync(cancellationToken);
        foreach (var c in claims)
        {
            Context.Set<TRoleClaim>().Remove(c);
        }
    }
}
