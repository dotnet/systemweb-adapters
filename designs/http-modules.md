# IHttpModules and Emulated Pipeline Support

> **Note**: This implementation is not tied to IIS and does not hook into any of IIS events if ran on IIS.

Support for `HttpApplication` and `IHttpModule` is emulated as best as possible on the ASP.NET Core pipeline. This is not tied to IIS and will work on Kestrel or any other host by using middleware to invoke the expected events at the times that best approximate the timing from ASP.NET Core. An attempt has been made to get the events to fire at the appropriate time, but because of the substantial difference between ASP.NET and ASP.NET Core there may still be unexpected behavior.

In order to register either an `HttpApplication` or `IHttpModule` instance, use the following pattern:

```csharp
using System.Web;
using ModulesLibrary;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSystemWebAdapters()
    // . AddHttpApplication(options => // Use non-generic version if no custom HttpApplication
    .AddHttpApplication<MyApp>(options =>
    {
        // Size of pool for HttpApplication instances. Should be what the expected concurrent requests will be
        options.PoolSize = 10;

        // Register a module by name
        options.RegisterModule<EventsModule>("Events");
    });

var app = builder.Build();

app.UseSystemWebAdapters();

app.Run();

class MyApp : HttpApplication
{
}

```

The normal `.UseSystemWebAdapters()` middleware builder will enable majority of the events. However, the authentication and authorization events require two additional middleware calls in order to enable them if you want the events to fire in the expected order. If they are omitted, they will be called at the point `UseSystemWebAdapters()` is added.


```diff
app.UseRouting();

app.UseAuthentication();
+ app.UseRaiseAuthenticationEvents();

app.UseAuthorization();
+ app.UseRaiseAuthorizationEvents();

app.UseSystemWebAdapters();
```

## When should this be used?

> Most of the time, this should not be used. Prefer direct ASP.NET Core middleware if possible.

This is intended mostly for scenarios where a module needs to be run on ASP.NET Core but is unable to be migrated easily. Ideally, the code in a module should be restructured to be used as middleware. This is especially recommended when only a single or few events are used; those can usually be migrated in a straightfoward way.

However, if a module has many thousands of line of code and many events being used (the initial driver of this feature), this can provide a stepping stone to migrating that functionality to ASP.NET Core.

## Emulated Events

> For details on how this worked in .NET Framework, see the [official documentation](https://learn.microsoft.com/en-us/dotnet/api/system.web.httpapplication)

The IIS event pipeline that is expected by `IHttpModule` and `HttpApplication` is emulated using middleware by the adapters. As part of this, it will add additional middleware that will invoke the events. This is done via a feature that is inserted early on in the adapter pipeline [IHttpApplicationFeature](../src/Microsoft.AspNetCore.SystemWebAdapters/Adapters/IHttpApplicationFeature.cs). This exposes the `HttpApplication` for the request, as well as the ability to raise events on it.

Events have a prescribed order which is replicated with these emulated events. However, because the rest of the ASP.NET Core pipeline is unaware of these events and so some of the state of the request may not be exactly replicated.

A common pattern is to be able to call `HttpRequest.End()` or `HttpApplication.CompleteRequest()`. Both of these are supported, as well as continuing to raise the events that are raised in IIS with this (including `EndRequest` and the logging events).

> Note: In the cases in which no modules or `HttpApplication` type is registered, the emulated pipeline is not added to the middleware chain.

## HttpApplication lifetime

On ASP.NET Framework, each request would get an individual `HttpApplication` instance. This object contains the following information:

- Event callbacks registered either on the `HttpApplication` type itself or on registered modules
- Any state contained in the `HttpApplication` instance or its registered modules

In order to support this, one of the first middlewares invoked will retrieve an instance of `HttpApplication`. This uses a `PooledObjectPolicy<HttpApplication>` that will create an instance of the application's `HttpApplication` type and register all modules on it. When the request is existing that middleware, it will return the `HttpApplication` instance. Upon return, it the `HttpContext` instance assigned to it is removed.

This can potentially create a number of instances of `HttpApplication` that are only used a limited number of times. The pool can be controlled by customizing the `HttpApplicationOptions.PoolSize` option.