# Microsoft.AspNetCore.SystemWebAdapters.Owin

This library provides OWIN integration for ASP.NET Core applications using System Web Adapters. It enables three distinct ways to incorporate OWIN middleware pipelines into your ASP.NET Core application, making it easier to migrate from ASP.NET Framework applications that use OWIN.

## Features

This library enables three key integration patterns:

1. **[OWIN Pipeline in Emulated HttpApplication Events](#1-owin-pipeline-in-emulated-httpapplication-events)** - Run OWIN middleware within the emulated `HttpApplication` pipeline, similar to how OWIN works in ASP.NET Framework
2. **[OWIN Pipeline as Main Pipeline Middleware](#2-owin-pipeline-as-main-pipeline-middleware)** - Add OWIN middleware directly to the ASP.NET Core middleware pipeline
3. **[OWIN Pipeline as Authentication Handler](#3-owin-pipeline-as-authentication-handler)** - Use OWIN middleware as an ASP.NET Core authentication handler

---

## 1. OWIN Pipeline in Emulated HttpApplication Events

This integration pattern allows you to run OWIN middleware within the emulated `HttpApplication` event pipeline, providing behavior similar to ASP.NET Framework's integrated pipeline mode.

### Overview

When using this approach, OWIN middleware is registered to execute at specific `HttpApplication` event stages (such as `AuthenticateRequest`, `AuthorizeRequest`, etc.). This is particularly useful when migrating applications that rely on the ASP.NET Framework integrated pipeline and OWIN working together.

### Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSystemWebAdapters()
    .AddOwinApp(app =>
    {
        // Configure OWIN middleware here
        // This will run within the HttpApplication event pipeline
        app.UseMyOwinMiddleware();
    });

var app = builder.Build();

// Enable the HttpApplication pipeline
app.UseSystemWebAdapters();

app.Run();
```

### With Service Provider Access

You can also access the `IServiceProvider` when configuring the OWIN pipeline:

```csharp
builder.Services
    .AddSystemWebAdapters()
    .AddOwinApp((app, services) =>
    {
        var someService = services.GetRequiredService<IMyService>();
        app.UseMyOwinMiddleware(someService);
        app.UseStageMarker(PipelineStage.Authenticate);
    });
```

### How It Works

The OWIN pipeline is integrated into the following `HttpApplication` events:

- `AuthenticateRequest` / `PostAuthenticateRequest`
- `AuthorizeRequest` / `PostAuthorizeRequest`
- `ResolveRequestCache` / `PostResolveRequestCache`
- `MapRequestHandler` / `PostMapRequestHandler`
- `AcquireRequestState` / `PostAcquireRequestState`
- `PreRequestHandlerExecute`

Each OWIN stage corresponds to a specific event in the `HttpApplication` lifecycle, ensuring familiar behavior for developers migrating from ASP.NET Framework.

The integration uses OWIN's `.UseStageMarker(PipelineStage)` extension method to mark pipeline stages. This allows OWIN middleware to be organized and executed at the appropriate points in the `HttpApplication` event lifecycle, just as it would in ASP.NET Framework's integrated pipeline mode.

---

## 2. OWIN Pipeline as Main Pipeline Middleware

This integration pattern allows you to add OWIN middleware directly to the ASP.NET Core middleware pipeline, without using the emulated `HttpApplication` events.

### Overview

Use this approach when you want to incorporate OWIN middleware into your ASP.NET Core application's standard middleware pipeline. This is useful for gradually migrating OWIN-based middleware to ASP.NET Core while maintaining compatibility.

### Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Add OWIN middleware to the main pipeline
app.UseOwin(owinApp =>
{
    // Configure OWIN middleware here
    owinApp.UseMyOwinMiddleware();
});

// Other ASP.NET Core middleware
app.UseRouting();
app.MapControllers();

app.Run();
```

### With Service Provider Access

```csharp
app.UseOwin((owinApp, services) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    owinApp.UseMyOwinMiddleware(configuration);
});
```

### How It Works

The OWIN middleware is bridged into the ASP.NET Core pipeline at the point where `UseOwin()` is called. The middleware processes requests using the standard OWIN `AppFunc` delegate pattern, converting between ASP.NET Core's `HttpContext` and OWIN's environment dictionary as needed.

---

## 3. OWIN Pipeline as Authentication Handler

This integration pattern allows you to use OWIN middleware as an ASP.NET Core authentication handler, enabling existing OWIN authentication middleware to work within the ASP.NET Core authentication system.

### Overview

This approach is ideal when migrating authentication logic from ASP.NET Framework applications that use OWIN authentication middleware (such as cookie authentication, OAuth providers, or custom authentication schemes). The OWIN pipeline runs as a proper ASP.NET Core authentication handler, integrating with the `AuthenticationBuilder` API.

### Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSystemWebAdapters()
    .AddAuthentication()
    .AddOwinAuthentication((owinApp, services) =>
    {
        // Configure OWIN authentication middleware here
        owinApp.UseCookieAuthentication(new CookieAuthenticationOptions
        {
            // OWIN authentication options
        });
    });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

### With Custom Authentication Scheme

You can specify a custom authentication scheme name:

```csharp
builder.Services
    .AddAuthentication()
    .AddOwinAuthentication("MyOwinScheme", (owinApp, services) =>
    {
        owinApp.UseMyOwinAuthenticationMiddleware();
    });
```

### How It Works

The OWIN authentication pipeline is wrapped in an ASP.NET Core `AuthenticationHandler`. When authentication is triggered (either automatically or via `[Authorize]` attributes), the OWIN middleware pipeline executes and any authentication results are properly integrated into ASP.NET Core's authentication system.

By default, the authentication scheme is registered as `OwinAuthenticationDefaults.AuthenticationScheme` unless a custom scheme name is provided.

### Accessing OWIN Authentication

You can continue to use the OWIN `IAuthenticationManager` interface within your application code by accessing it through the OWIN context:

```csharp
var owinContext = HttpContext.GetOwinContext();
var authManager = owinContext.Authentication;

// Use OWIN authentication methods
authManager.SignIn(identity);
authManager.SignOut();
```

This allows you to maintain existing authentication code that relies on OWIN's authentication APIs while gradually migrating to ASP.NET Core patterns.

### Example: Using with ASP.NET Framework Identity

A common scenario is migrating an application that uses ASP.NET Framework Identity with OWIN cookie authentication. Here's a complete example:

#### Program.cs Configuration

```csharp
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Interop;
using Owin;

var builder = WebApplication.CreateBuilder(args);

// Configure data protection to match ASP.NET Framework settings
var sharedApplicationName = "CommonMvcAppName";
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "sharedkeys", sharedApplicationName)))
    .SetApplicationName(sharedApplicationName);

// Add OWIN authentication handler
builder.Services
    .AddAuthentication()
    .AddOwinAuthentication("SharedCookie", (app, services) =>
    {
        // Register OWIN services per request
        app.CreatePerOwinContext(ApplicationDbContext.Create);
        app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
        app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

        // Configure data protector for cookie sharing
        var dataProtector = services.GetDataProtector(
            "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
            "SharedCookie",
            "v2");

        app.UseCookieAuthentication(new CookieAuthenticationOptions
        {
            AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
            LoginPath = new PathString("/Account/Login"),
            Provider = new CookieAuthenticationProvider
            {
                OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                    validateInterval: TimeSpan.FromMinutes(30),
                    regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
            },
            CookieName = ".AspNet.ApplicationCookie",
            TicketDataFormat = new AspNetTicketDataFormat(new DataProtectorShim(dataProtector))
        });
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();

app.Run();
```

#### Using in Controllers

```csharp
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Owin.Security;

[Authorize]
public class AccountController : Controller
{
    // Access OWIN-registered services via GetOwinContext()
    public ApplicationSignInManager SignInManager =>
        HttpContext.GetOwinContext().Get<ApplicationSignInManager>();

    public ApplicationUserManager UserManager =>
        HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

    private IAuthenticationManager AuthenticationManager =>
        HttpContext.GetOwinContext().Authentication;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Use OWIN Identity services directly
        var result = await SignInManager.PasswordSignInAsync(
            model.Email, 
            model.Password, 
            model.RememberMe, 
            shouldLockout: false);

        if (result == SignInStatus.Success)
        {
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Invalid login attempt.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        // Sign out using OWIN authentication
        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        return RedirectToAction("Index", "Home");
    }
}
```

#### Key Points

1. **Data Protection Configuration**: The data protection settings must match between ASP.NET Framework and ASP.NET Core for cookie sharing to work properly. Use the same `ApplicationName` and key storage location.

2. **CreatePerOwinContext**: Use `app.CreatePerOwinContext<T>()` to register services that should be created once per request, such as `ApplicationUserManager` and `ApplicationSignInManager`.

3. **DataProtectorShim**: Use `AspNetTicketDataFormat` with `DataProtectorShim` to bridge ASP.NET Core's `IDataProtector` to OWIN's data protection system.

4. **GetOwinContext()**: Access OWIN-registered services and the `IAuthenticationManager` through `HttpContext.GetOwinContext()`.

5. **Cookie Name Matching**: Ensure the `CookieName` matches between both applications if sharing authentication state.

This pattern allows you to reuse existing ASP.NET Framework Identity infrastructure while migrating to ASP.NET Core, providing a smooth transition path.
