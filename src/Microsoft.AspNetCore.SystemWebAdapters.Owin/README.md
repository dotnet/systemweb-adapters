# Microsoft.AspNetCore.SystemWebAdapters.Owin

Enables OWIN middleware to run in ASP.NET Core applications during incremental migration from ASP.NET Framework.

## Getting Started

Three integration patterns are available:

**1. OWIN in HttpApplication Events** (matches ASP.NET Framework behavior):

```csharp
builder.Services.AddSystemWebAdapters()
    .AddOwinApp(app =>
    {
        app.UseMyOwinMiddleware();
    });

var app = builder.Build();
app.UseSystemWebAdapters();
app.Run();
```

**2. OWIN as Pipeline Middleware**:

```csharp
app.UseOwin(owinApp =>
{
    owinApp.UseMyOwinMiddleware();
});
```

**3. OWIN as Authentication Handler**:

```csharp
builder.Services.AddAuthentication()
    .AddOwinAuthentication(options =>
    {
        options.AppBuilder = owinApp =>
        {
            owinApp.UseCookieAuthentication(new CookieAuthenticationOptions());
        };
    });
```

## Additional Documentation

For detailed examples and advanced scenarios, see the [full documentation in the repository](https://github.com/dotnet/systemweb-adapters/tree/main/src/Microsoft.AspNetCore.SystemWebAdapters.Owin).

- [Incremental Migration](https://learn.microsoft.com/aspnet/core/migration/inc/overview)
- [GitHub Repository](https://github.com/dotnet/systemweb-adapters)
