# Microsoft.AspNetCore.SystemWebAdapters.CoreServices

Provides services and middleware for using System Web Adapters in ASP.NET Core applications. Includes session management, remote app integration, HTTP handlers/modules, and more.

## Getting Started

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSystemWebAdapters();

var app = builder.Build();
app.UseRouting();
app.UseSystemWebAdapters();
app.Run();
```

## Usage

### Session State

**Local session** (backed by ASP.NET Core):

```csharp
builder.Services.AddSystemWebAdapters()
    .AddWrappedAspNetCoreSession();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
```

**Remote session** (shared with ASP.NET Framework):

```csharp
builder.Services.AddSystemWebAdapters()
    .AddRemoteAppClient(options =>
    {
        options.RemoteAppUrl = new Uri("https://framework-app.example.com");
        options.ApiKey = "your-api-key";
    })
    .AddSessionClient();
```

### HTTP Handlers

```csharp
app.MapHttpHandler<MyCustomHandler>("/handler.ashx");

public class MyCustomHandler : IHttpHandler
{
    public bool IsReusable => true;
    
    public void ProcessRequest(System.Web.HttpContext context)
    {
        context.Response.Write("Hello from IHttpHandler!");
    }
}
```

### HTTP Modules

```csharp
builder.Services.AddSystemWebAdapters()
    .AddHttpModules(modules => modules.Add<MyCustomModule>());
```

## Additional Documentation

- [Get Started with Incremental Migration](https://learn.microsoft.com/aspnet/core/migration/inc/start)
- [Remote Session](https://learn.microsoft.com/aspnet/core/migration/inc/remote-session)
- [Remote Authentication](https://learn.microsoft.com/aspnet/core/migration/inc/remote-authentication)
- [GitHub Repository](https://github.com/dotnet/systemweb-adapters)
