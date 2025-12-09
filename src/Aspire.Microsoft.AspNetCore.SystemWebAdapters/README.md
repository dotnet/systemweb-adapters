# Aspire.Microsoft.AspNetCore.SystemWebAdapters

Provides .NET Aspire integration for System Web Adapters, automatically configuring settings based on Aspire-provided environment variables.

## Getting Started

**In your Aspire app host:**

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpressProject<Projects.MyFrameworkApp>("framework");

var coreApp = builder.AddProject<Projects.MyCoreApp>("core")
    .WithIncrementalMigrationFallback(frameworkApp, options =>
    {
        options.RemoteSession = RemoteSession.Enabled;
        options.RemoteAuthentication = RemoteAuthentication.DefaultScheme;
    });

builder.Build().Run();
```

**In your ASP.NET Core app:**

```csharp
// Automatically configured from Aspire environment
builder.AddSystemWebAdapters();
```

**In your ASP.NET Framework app (Global.asax.cs):**

```csharp
protected void Application_Start()
{
    HttpApplicationHost.RegisterHost(builder =>
    {
        builder.AddSystemWebAdapters(); // Auto-configured
    });
}
```

## Additional Documentation

- [Incremental Migration](https://learn.microsoft.com/aspnet/core/migration/inc/overview)
- [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/)
- [GitHub Repository](https://github.com/dotnet/systemweb-adapters)
