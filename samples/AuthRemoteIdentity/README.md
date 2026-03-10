# ASP.NET Identity Migration Options

This sample demonstrates four migration strategies using an Aspire AppHost (`AuthRemoteIdentityAppHost`) that runs the legacy ASP.NET Framework app alongside multiple ASP.NET Core variants against a shared SQL Server database.

## 1. Remote ASP.NET Identity with YARP Fallback

**What it is:** Keep authentication/Identity in the legacy ASP.NET Framework app, and have the ASP.NET Core app proxy (or fall back) to the legacy app for auth-related endpoints during the migration.

**Sample:** `core`

### Pros
- **Incremental migration**: Keeps the existing ASP.NET (System.Web) Identity implementation while you move pages/endpoints to ASP.NET Core
- **Minimal database changes**: Legacy Identity store remains the source of truth
- **Lower initial risk**: Authentication behavior stays largely unchanged while you modernize the app surface

### Cons
- **Dual runtime required**: Must run and operate the legacy app alongside the new ASP.NET Core app
- **Network complexity**: Proxying/fallback introduces routing overhead, latency, and new failure modes
- **Debugging challenges**: Auth flows span two apps, making cookies/redirects harder to trace end-to-end

## 2. OWIN Pipeline in ASP.NET Core

**What it is:** Host your existing OWIN authentication pipeline inside the ASP.NET Core app so you can reuse your current Identity/auth middleware with minimal changes.

**Sample:** `owin`

### Pros
- **Reuse existing middleware**: OWIN-based Identity/auth middleware and configuration require less rewrite
- **Unblocks Core migration**: Move to ASP.NET Core without immediately changing the Identity data model

### Cons
- **Carries technical debt**: Legacy OWIN/Identity dependencies and patterns you'll likely want to retire later
- **Less native Core experience**: Can limit how natural the app feels in ASP.NET Core (DI, middleware ordering, modern auth patterns)

## 3. EF6-Backed Data Store for ASP.NET Core Identity

**What it is:** Run ASP.NET Core Identity, but implement its user/role stores on top of Entity Framework 6 so you can keep using the existing EF6 model and database while migrating.

**Sample:** `identityef6`

### Pros
- **Preserve existing schema**: ASP.NET Core Identity uses an EF6-backed store, allowing you to keep the existing database longer
- **Defer EF Core migration**: Modernize the web app without immediately migrating to EF Core

### Cons
- **Still on EF6**: Tooling and feature set differs from EF Core, so this is typically an intermediate step
- **Compatibility concerns**: Potential operational complexity from running EF6 in a modern ASP.NET Core stack

> [!NOTE]
> The implementation for this is [here](../../src/Microsoft.AspNetCore.Identity.EntityFramework6/) and is currently a POC of what it might look like

## 4. Full Migration to EF Core and ASP.NET Core Identity

**What it is:** Fully adopt ASP.NET Core Identity with an EF Core model/store, migrating schema/data as needed to reach the long-term "all-in" ASP.NET Core architecture.

### Pros
- **Clean end state**: First-class ASP.NET Core Identity + EF Core integration with modern tooling
- **Simplified architecture**: Removes legacy runtime dependencies and technical debt

### Cons
- **High upfront investment**: Requires careful data/schema migration planning and comprehensive testing
- **Deployment coordination**: May require coordinated rollout to avoid disrupting sign-in sessions and user flows