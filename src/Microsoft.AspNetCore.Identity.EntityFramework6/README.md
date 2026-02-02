# Microsoft.AspNetCore.Identity.EntityFramework6

This package provides a custom data store implementation for **ASP.NET Core Identity** that uses **Entity Framework 6** (EF6) as the database access technology.

## Important: Entity Framework 6, NOT Entity Framework Core

⚠️ **This package uses Entity Framework 6 (System.Data.Entity), NOT Entity Framework Core (Microsoft.EntityFrameworkCore).**

This is a critical distinction:
- **Entity Framework 6** is the legacy, .NET Framework-era ORM that uses `System.Data.Entity`
- **Entity Framework Core** is the modern, cross-platform ORM that uses `Microsoft.EntityFrameworkCore`

## Purpose

This package enables ASP.NET Core applications to reuse existing Entity Framework 6 database contexts and schemas that were created by ASP.NET Framework Identity (the legacy Identity system). This is particularly useful for:

- **Migration scenarios**: When migrating from ASP.NET Framework to ASP.NET Core but keeping the same database
- **Hybrid applications**: When running both ASP.NET Framework and ASP.NET Core applications against the same database
- **Legacy schema compatibility**: When you need to maintain the exact database schema created by ASP.NET Framework Identity

## Key Components

### `IdentityEF6DbContext<TUser>`

Base class for an EF6 database context configured for ASP.NET Core Identity. It uses Entity Framework 6's `DbContext` and Fluent API to map to the same schema that ASP.NET Framework Identity creates:

- `AspNetUsers` table
- `AspNetRoles` table
- `AspNetUserClaims` table
- `AspNetUserLogins` table
- `AspNetUserRoles` table
- `AspNetUserTokens` table
- `AspNetRoleClaims` table

### `IdentityEF6DbContext<TUser, TRole, TKey>`

Generic version that allows customization of user types, role types, and primary key types while maintaining compatibility with the ASP.NET Framework Identity schema.

## Usage Example

```csharp
// Define your user class
public class ApplicationUser : IdentityUser<string>
{
    // Add custom properties as needed
}

// Create your DbContext
public class ApplicationDbContext : IdentityEF6DbContext<ApplicationUser>
{
    public ApplicationDbContext(string connectionString) 
        : base(connectionString)
    {
    }
}

// Configure in ASP.NET Core
services.AddDbContext<ApplicationDbContext>(options =>
{
    // Entity Framework 6 configuration
});

services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

## Technology Stack

- **ASP.NET Core Identity**: Modern identity management system
- **Entity Framework 6**: Legacy ORM (`System.Data.Entity` namespace)
- **Target**: .NET Framework schema compatibility

## When NOT to Use This Package

If you're building a new ASP.NET Core application from scratch, you should use the standard `Microsoft.AspNetCore.Identity.EntityFrameworkCore` package, which uses **Entity Framework Core**. Only use this package if you specifically need Entity Framework 6 compatibility.
