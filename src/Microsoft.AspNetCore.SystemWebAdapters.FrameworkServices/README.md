# Microsoft.AspNetCore.SystemWebAdapters.FrameworkServices

Provides services for ASP.NET Framework applications to integrate with System Web Adapters, enabling session/authentication sharing with ASP.NET Core apps during incremental migration.

## Getting Started

The package automatically adds an HTTP module to your `web.config`.

In `Global.asax.cs`:

```csharp
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;

public class MvcApplication : HttpApplication
{
    protected void Application_Start()
    {
        HttpApplicationHost.RegisterHost(builder =>
        {
            builder.AddSystemWebAdapters();
        });
        
        // ... other startup code
    }
}
```
or
```csharp
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;

public class MvcApplication : HttpApplication
{
    protected void Application_Start()
    {
        var builder = CreateBuilder();
        builder.AddSystemWebAdapters();
        builder.BuildAndRunInBackground();

        // ... other startup code
    }
}
```

### Remote Session Server

Share session state with an ASP.NET Core application:

```csharp
HttpApplicationHost.RegisterHost(builder =>
{
    builder.AddSystemWebAdapters()
        .AddRemoteAppServer(options => options.ApiKey = "your-api-key")
        .AddSessionServer();
});
```

### Shared Data Protection

Share encryption keys with ASP.NET Core:

```csharp
using Microsoft.AspNetCore.DataProtection;

HttpApplicationHost.RegisterHost(builder =>
{
    builder.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(@"C:\shared-keys"))
        .SetApplicationName("MySharedApp");
    
    builder.AddSystemWebAdapters();
});
```

## Additional Documentation

- [Get Started with Incremental Migration](https://learn.microsoft.com/aspnet/core/migration/inc/start)
- [Remote Session](https://learn.microsoft.com/aspnet/core/migration/inc/remote-session)
- [Remote Authentication](https://learn.microsoft.com/aspnet/core/migration/inc/remote-authentication)
- [GitHub Repository](https://github.com/dotnet/systemweb-adapters)
