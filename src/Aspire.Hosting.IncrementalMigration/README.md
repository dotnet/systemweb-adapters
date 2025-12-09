# Aspire.Hosting.IncrementalMigration

Provides .NET Aspire hosting extensions to run ASP.NET Framework and ASP.NET Core applications side-by-side during incremental migration.

## Getting Started

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add ASP.NET Framework app with IIS Express
var frameworkApp = builder.AddIISExpressProject<Projects.MyFrameworkApp>("framework");

// Add ASP.NET Core app with fallback to Framework app
var coreApp = builder.AddProject<Projects.MyCoreApp>("core")
    .WithIncrementalMigrationFallback(frameworkApp, options =>
    {
        options.RemoteSession = RemoteSession.Enabled;
        options.RemoteAuthentication = RemoteAuthentication.DefaultScheme;
    });

builder.Build().Run();
```

## Features

- **IIS Express Hosting**: Run ASP.NET Framework projects from Aspire with Visual Studio debugging support
- **Fallback Routing**: Route unimplemented requests from Core to Framework app
- **Session Sharing**: Share session state between Framework and Core apps
- **Authentication Sync**: Synchronize authentication state across applications

## Platform Requirements

- **Windows**: Required for IIS Express
- **.NET 8+**: For Aspire host
- **Visual Studio**: For debugging support

## Additional Documentation

- [Incremental Migration](https://learn.microsoft.com/aspnet/core/migration/inc/overview)
- [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/)
- [GitHub Repository](https://github.com/dotnet/systemweb-adapters)

> Inspired by [C3D.Extensions.Aspire](https://github.com/CZEMacLeod/C3D.Extensions.Aspire)
