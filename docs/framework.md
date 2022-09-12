# ASP.NET Framework Configuration

In order to setup the adapters for usage within an ASP.NET framework app, a module is inserted when the package is installed:

```xml
<configuration>
  ...
  <system.webServer>
    <modules>
      <remove name="SystemWebAdapterModule" />
      <add name="SystemWebAdapterModule" type="Microsoft.AspNetCore.SystemWebAdapters.SystemWebAdapterModule, Microsoft.AspNetCore.SystemWebAdapters" preCondition="managedHandler" />
    </modules>
  </system.webServer>
  ...
</configuration>
```

This module will only run when in the context of a managed application and only if configured to do so. This configuration should take place in `Global.asax.cs` or `Global.asax.vb` and would look like the following:

```cs
protected void Application_Start()
{
    AreaRegistration.RegisterAllAreas();
    GlobalConfiguration.Configure(WebApiConfig.Register);
    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
    RouteConfig.RegisterRoutes(RouteTable.Routes);
    BundleConfig.RegisterBundles(BundleTable.Bundles);

    SystemWebAdapterConfiguration.AddSystemWebAdapters(this)
        .AddProxySupport(options => options.UseForwardedHeaders = true)
        .AddJsonSessionSerializer(options => ClassLibrary.SessionUtils.RegisterSessionKeys(options.KnownKeys))
        .AddRemoteAppServer(options => options.ApiKey = ClassLibrary.SessionUtils.ApiKey)
        .AddSessionServer();
}
```

Customizing the adapters is done by adding modules to via `Application.AddSystemWebAdapters()` which will be loaded within the `SystemWebAdaptersModule` registered in `web.config`. The available configuration right now are described below.

## Additional Modules

### Proxy Support

During migration, the .NET Framework application will be moved and will be downstream from a reverse proxy. In order for things like port, URL, and scheme to be constructed correctly, we must update the values `HttpContext` use. This is similar to the [process used on ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer).

This is done by calling this `ISystemWebAdapterBuilder.AddProxySupport` method and configuring things as desired.

### Remote App Session Support

In order to support session, an option is to retrieve it from the framework application. For details on how this works and how to configure it, please see [here](session-state/remote-session.md).
