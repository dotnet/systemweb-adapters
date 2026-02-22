// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Identity.EntityFramework6;

/// <summary>
/// Default IdentityDbContext that uses the default entity types for ASP.NET Identity Users, Roles, Claims, Logins. 
/// Use this overload to add your own entity types.
/// </summary>
public class IdentityEF6DbContext :
    IdentityEF6DbContext<IdentityUserEF6, IdentityRoleEF6, string, IdentityUserLoginEF6, IdentityUserRoleEF6, IdentityUserClaimEF6>
{
    /// <summary>
    ///     Default constructor which uses the DefaultConnection
    /// </summary>
    public IdentityEF6DbContext()
        : base()
    {
    }

    /// <summary>
    ///     Constructor which takes the connection string to use
    /// </summary>
    /// <param name="nameOrConnectionString"></param>
    public IdentityEF6DbContext(string nameOrConnectionString)
        : base(nameOrConnectionString)
    {
    }
}

/// <summary>
///     DbContext which uses a custom user entity with a string primary key
/// </summary>
/// <typeparam name="TUser"></typeparam>
public class IdentityEF6DbContext<TUser> :
    IdentityEF6DbContext<TUser, IdentityRoleEF6, string, IdentityUserLoginEF6, IdentityUserRoleEF6, IdentityUserClaimEF6>
    where TUser : IdentityUserEF6
{
    /// <summary>
    ///     Default constructor which uses the DefaultConnection
    /// </summary>
    public IdentityEF6DbContext()
        : this("DefaultConnection")
    {
    }

    /// <summary>
    ///     Constructor which takes the connection string to use
    /// </summary>
    /// <param name="nameOrConnectionString"></param>
    public IdentityEF6DbContext(string nameOrConnectionString)
        : base(nameOrConnectionString)
    {
    }

    internal static bool IsIdentityV1Schema(DbContext db)
    {
        var originalConnection = db.Database.Connection as SqlConnection;
        // Give up and assume its ok if its not a sql connection
        if (originalConnection == null)
        {
            return false;
        }

        if (db.Database.Exists())
        {
            using (var tempConnection = new SqlConnection(originalConnection.ConnectionString))
            {
                tempConnection.Open();
                return
                    VerifyColumns(tempConnection, "AspNetUsers", "Id", "UserName", "PasswordHash", "SecurityStamp",
                        "Discriminator") &&
                    VerifyColumns(tempConnection, "AspNetRoles", "Id", "Name") &&
                    VerifyColumns(tempConnection, "AspNetUserRoles", "UserId", "RoleId") &&
                    VerifyColumns(tempConnection, "AspNetUserClaims", "Id", "ClaimType", "ClaimValue", "User_Id") &&
                    VerifyColumns(tempConnection, "AspNetUserLogins", "UserId", "ProviderKey", "LoginProvider");
            }
        }

        return false;
    }

    [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities",
        Justification = "Reviewed")]
    internal static bool VerifyColumns(SqlConnection conn, string table, params string[] columns)
    {
        var tableColumns = new List<string>();
        using (
            var command =
                new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS where TABLE_NAME=@Table", conn))
        {
            command.Parameters.Add(new SqlParameter("Table", table));
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Add all the columns from the table
                    tableColumns.Add(reader.GetString(0));
                }
            }
        }
        // Make sure that we find all the expected columns
        return columns.All(tableColumns.Contains);
    }
}


/// <summary>
/// Base class for the Entity Framework 6 database context used for Identity Core with custom user and role types.
/// </summary>
/// <typeparam name="TUser">The type of user objects.</typeparam>
/// <typeparam name="TRole">The type of role objects.</typeparam>
/// <typeparam name="TKey">The type of the primary key for users and roles.</typeparam>
/// <typeparam name="TUserLogin">The type representing a user login.</typeparam>
/// <typeparam name="TUserRole">The type representing a user role.</typeparam>
/// <typeparam name="TUserClaim">The type representing a user claim.</typeparam>
public class IdentityEF6DbContext<TUser, TRole, TKey, TUserLogin, TUserRole, TUserClaim> : DbContext
    where TUser : IdentityUserEF6<TKey, TUserLogin, TUserRole, TUserClaim>
    where TRole : IdentityRoleEF6<TKey, TUserRole>
    where TKey : IEquatable<TKey>
    where TUserLogin : IdentityUserLoginEF6<TKey>
    where TUserRole : IdentityUserRoleEF6<TKey>
    where TUserClaim : IdentityUserClaimEF6<TKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityEF6DbContext{TUser, TRole, TKey}"/> class.
    /// </summary>
    public IdentityEF6DbContext() : base("DefaultConnection")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityEF6DbContext{TUser, TRole, TKey}"/> class using a connection string.
    /// </summary>
    /// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
    public IdentityEF6DbContext(string nameOrConnectionString) : base(nameOrConnectionString)
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TUser}"/> of Users.
    /// </summary>
    public virtual DbSet<TUser> Users { get; set; } = default!;

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TRole}"/> of Roles.
    /// </summary>
    public virtual DbSet<TRole> Roles { get; set; } = default!;

    /// <summary>
    /// Configures the schema needed for the identity framework using EF6's fluent API.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        // Needed to ensure subclasses share the same table
        var user = modelBuilder.Entity<TUser>()
            .ToTable("AspNetUsers");
        user.HasMany(u => u.Roles).WithRequired().HasForeignKey(ur => ur.UserId);
        user.HasMany(u => u.Claims).WithRequired().HasForeignKey(uc => uc.UserId);
        user.HasMany(u => u.Logins).WithRequired().HasForeignKey(ul => ul.UserId);
        user.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(256)
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UserNameIndex") { IsUnique = true }));

        // Map NormalizedUserName to UserName column - normalization handled in queries
        user.Ignore(u => u.NormalizedUserName);

        // CONSIDER: u.Email is Required if set on options?
        user.Property(u => u.Email).HasMaxLength(256);

        // Map NormalizedEmail to Email column - normalization handled in queries
        user.Ignore(u => u.NormalizedEmail);

        // Ignore ConcurrencyStamp as ASP.NET Identity v1 doesn't have it
        user.Ignore(u => u.ConcurrencyStamp);

        modelBuilder.Entity<TUserRole>()
            .HasKey(r => new { r.UserId, r.RoleId })
            .ToTable("AspNetUserRoles");

        modelBuilder.Entity<TUserLogin>()
            .HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId })
            .ToTable("AspNetUserLogins");

        modelBuilder.Entity<TUserClaim>()
            .ToTable("AspNetUserClaims");

        var role = modelBuilder.Entity<TRole>()
            .ToTable("AspNetRoles");
        role.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(256)
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("RoleNameIndex") { IsUnique = true }));

        // Map NormalizedName to Name column - normalization handled in queries
        role.Ignore(r => r.NormalizedName);

        // Ignore ConcurrencyStamp for roles as ASP.NET Identity v1 doesn't have it
        role.Ignore(r => r.ConcurrencyStamp);

        role.HasMany(r => r.Users).WithRequired().HasForeignKey(ur => ur.RoleId);
    }
}
