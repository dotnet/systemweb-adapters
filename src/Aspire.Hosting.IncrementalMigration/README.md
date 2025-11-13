# Aspire.Hosting.IncrementalMigration

This project provides .NET Aspire hosting extensions to support **incremental migration** scenarios when modernizing ASP.NET Framework applications to ASP.NET Core.

> **Inspiration**: This project builds on the initial exploration and concepts developed in [C3D.Extensions.Aspire](https://github.com/CZEMacLeod/C3D.Extensions.Aspire).

## Overview

The `Aspire.Hosting.IncrementalMigration` library enables you to run ASP.NET Framework and ASP.NET Core applications side-by-side in a .NET Aspire orchestrated environment, with support for:

- **IIS Express Hosting**: Run ASP.NET Framework projects directly from Aspire using IIS Express
- **Incremental Migration Fallback**: Configure ASP.NET Core apps to fall back to ASP.NET Framework apps for unimplemented routes
- **Remote Session Sharing**: Share session state between Framework and Core applications
- **Remote Authentication**: Synchronize authentication state across both applications

## Key Features

### IIS Express Project Support

Add ASP.NET Framework projects to your Aspire app host using `AddIISExpressProject`:

```csharp
var frameworkApp = builder.AddIISExpressProject<Projects.MyFrameworkApp>("framework")
    .WithHttpHealthCheck("/health");
```

This extension:
- Automatically configures IIS Express with the correct bindings and ports
- Integrates with Visual Studio debugging on Windows
- Generates temporary IIS configuration files
- Supports OTLP telemetry export

### Incremental Migration Fallback

Configure a modern ASP.NET Core app to fall back to a legacy Framework app using `WithIncrementalMigrationFallback`:

```csharp
var coreApp = builder.AddProject<Projects.MyCoreApp>("core")
    .WithIncrementalMigrationFallback(frameworkApp, options =>
    {
        options.RemoteSession = RemoteSession.Enabled;
        options.RemoteAuthentication = RemoteAuthentication.DefaultScheme;
    });
```

This automatically configures:
- Remote application URLs and API keys
- Environment variables for both applications
- Session state sharing (when enabled)
- Authentication synchronization (when enabled)

## Use Cases

This library is ideal for organizations that need to:

1. **Gradually migrate** large ASP.NET Framework applications to ASP.NET Core
2. **Run hybrid deployments** with some features on Core and others on Framework
3. **Test migrations** in a unified development environment
4. **Share state** (sessions, authentication) during the migration period

## Architecture

### Visual Studio Debugger Attachment

> **Note**: This custom debugger attachment is necessary because Aspire's DCP (Distributed Control Plane) doesn't currently support attaching debuggers to arbitrary processes. The extension works around this limitation by directly automating Visual Studio through COM interop.

When running IIS Express projects from Aspire, the debugger is automatically attached through a multi-step process:

1. **Detection**: On resource startup, the extension subscribes to resource lifecycle events to detect when the IIS Express process starts
2. **COM Discovery**: Uses the Windows Running Object Table (ROT) to enumerate active Visual Studio instances via COM interop
3. **Instance Matching**: Locates the VS instance debugging the current Aspire host by matching process IDs
4. **DTE Automation**: Uses the EnvDTE object model (manually defined COM interfaces to avoid dependencies) to programmatically attach the debugger
5. **Process Attachment**: Attaches to the IIS Express process with the appropriate debug engines (native + managed)

The implementation uses:
- **COM/ROT**: For discovering running VS instances
- **EnvDTE**: VS automation object model (custom interface definitions)
- **STA Thread + Message Pump**: Required for COM interop with Visual Studio
- **Aspire Resource Events**: For timing the attachment to the IIS process lifecycle

This provides the same debugging experience as running IIS Express directly from Visual Studio, while being fully orchestrated by Aspire.

## Platform Requirements

- **Windows**: Required for running IIS Express projects
- **.NET 8+**: For the Aspire host application
- **IIS Express**: Automatically detected from Visual Studio installation
