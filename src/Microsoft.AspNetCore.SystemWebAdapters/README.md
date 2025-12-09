# Microsoft.AspNetCore.SystemWebAdapters

Provides `System.Web` compatibility APIs for ASP.NET Core applications, enabling incremental migration from ASP.NET Framework. Use familiar types like `HttpContext`, `HttpRequest`, `HttpResponse`, and more in your ASP.NET Core apps and .NET Standard libraries.

## Getting Started

Configure in your ASP.NET Core `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSystemWebAdapters();

var app = builder.Build();
app.UseSystemWebAdapters();
app.Run();
```

## Usage

Use `System.Web` types in your ASP.NET Core application:

```csharp
using System.Web;

var context = HttpContext.Current;
var userAgent = context.Request.UserAgent;
var ipAddress = context.Request.UserHostAddress;

// Access session and cache
context.Session?["Key"] = "Value";
context.Cache["CacheKey"] = "CacheValue";
```

**Supported APIs**: `HttpContext`, `HttpRequest`, `HttpResponse`, `HttpSessionState`, `Cache`, `HttpServerUtility`, `IHttpHandler`, `IHttpModule`, and more.

## Additional Documentation

- [Get Started with Incremental Migration](https://learn.microsoft.com/aspnet/core/migration/inc/start)
- [Incremental Migration Overview](https://learn.microsoft.com/aspnet/core/migration/inc/overview)
- [GitHub Repository](https://github.com/dotnet/systemweb-adapters)
