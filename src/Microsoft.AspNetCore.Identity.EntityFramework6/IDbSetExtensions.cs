// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Entity;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity.EntityFramework6;

/// <summary>
/// Extension methods for <see cref="IDbSet{TEntity}"/>.
/// </summary>
internal static class IDbSetExtensions
{
    /// <summary>
    /// If possible asynchronously finds an entity with the given primary key values 
    /// otherwise finds the entity synchronously.  
    /// If an entity with the given primary key values exists in the context, then it is
    /// returned immediately without making a request to the store. Otherwise, a
    /// request is made to the store for an entity with the given primary key values
    /// and this entity, if found, is attached to the context and returned. If no
    /// entity is found in the context or the store, then null is returned.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to find.</typeparam>
    /// <param name="this">The DbSet to search.</param>
    /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
    /// <returns>A task that represents the asynchronous find operation. The task result contains 
    /// the entity found, or null.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when multiple entities are found with the given primary key values.</exception>
    public static async Task<TEntity?> FindAsync<TEntity>(this IDbSet<TEntity> @this, params object[] keyValues)
        where TEntity : class
    {
        if (@this is DbSet<TEntity> thisDbSet)
        {
            return await thisDbSet.FindAsync(keyValues);
        }
        else
        {
            return @this.Find(keyValues);
        }
    }
}
