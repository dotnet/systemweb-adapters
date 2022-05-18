# Microsoft.AspNetCore.SystemWebAdapters

This project provides a collection of adapters that help migrating from `System.Web.dll` based ASP.NET projects to ASP.NET Core projects. Adapters currently include:

- `Microsoft.AspNetCore.SystemWebAdapters`: Subset of the APIs from `System.Web.dll` backed by `Microsoft.AspNetCore.Http` types
- `Microsoft.AspNetCore.SystemWebAdapters.SessionState`: Support for `System.Web.HttpContext.Session` usage

## Examples

A common use case that this is aimed at solving is in a project with multiple class libraries. Let's take a look at an example using the proposed adapters moving from .NET Framework to ASP.NET Core.

### ASP.NET Framework
Consider a controller that does something such as:

```cs
public class SomeController : Controller
{
  public ActionResult Index()
  {
    SomeOtherClass.SomeMethod(HttpContext.Current);
  }
}
```

which then has logic in a separate assembly passing that `HttpContext` around until finally, some inner method does some logic on it such as:

```cs
public class Class2
{
  public bool PerformSomeCheck(HttpContext context)
  {
    return context.Request.Headers["SomeHeader"] == "ExpectedValue";
  }
}
```

### ASP.NET Core

In order to run the above logic in ASP.NET Core, a developer will need to add the `Microsoft.AspNetCore.SystemWebAdapters` package, that will enable the projects to work on both platforms.

The libraries would need to be updated to understand the adapters, but it will be as simple as adding the package and recompiling. If these are the only dependencies a system has on `System.Web.dll`, then the libraries will be able to target .NET Standard to facilitate a simpler building process while migrating.

The controller in ASP.NET Core will now look like this:

```cs
public class SomeController : Controller
{
  [Route("/")]
  public IActionResult Index()
  {
    SomeOtherClass.SomeMethod(Context);
  }
}
```

Notice that since there's a `Controller.Context` property, they can pass that through, but it generally looks the same. Using implicit conversions, the `Microsoft.AspNetCore.Http.HttpContext` can be converted into the adapter that could then be passed around through the levels utilizing the code in the same way.

## Set up
Below are the steps needed to start using these adapters in your project:

1. Set up `NuGet.config` to point to the CI feed:
  ```xml
  <?xml version="1.0" encoding="utf-8"?>
  <configuration>
    <packageSources>
      <!--To inherit the global NuGet package sources remove the <clear/> line below -->
      <clear />
      <add key="nuget" value="https://api.nuget.org/v3/index.json" />
      <add key=".NET Libraries Daily" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-libraries/nuget/v3/index.json" />
    </packageSources>
  </configuration>
  ```
2. Install `Microsoft.AspNetCore.SystemWebAdapters`
3. If you use `HttpContext.Session`, install `Microsoft.AspNetCore.SystemWebAdapters.SessionState`
4. In your framework application:
   - The package installation will add a new module to your `web.config`. This module handles any customizations that are required to help migrate to .NET Core. See [this](docs/framework.md) for details on what is available here.
5. In your class libraries:
   - Class libraries can target .NET Standard 2.0 if desired which will ensure you are using the shared surface area
   - If you find that there's still some missing APIs, you may cross-compile with .NET Framework to maintain that behavior and handle it in .NET core in some other way
   - There should be no manual changes to enable using supported surface area of the adapters. If a member is not found, it is not currently supported on ASP.NET Core
6. For your ASP.NET Core application:
   - Register the adapter services:
    ```cs
    builder.Services.AddSystemWebAdapters();
    ``` 
   - Add the middleware after routing but before endpoints (if present);
   ```cs
   app.UseSystemWebAdapters();
   ```
   - For additional configuration, please see the [configuration](#configuration) section

## Supported Targets
- .NET Core App 3.1: This will implement the adapters against ASP.NET Core `HttpContext`. This will provide the following:
  - Conversions between ASP.NET Core `HttpContext` and `System.Web` adapter `HttpContext` (with appropriate caching so it will not cause perf hits for GC allocations)
  - Default implementations against `Microsoft.AspNetCore.Http.HttpContext`
  - Services that can be implemented to override some functionality such as session/caching/etc that may need to be customized to match experience.
- .NET Standard 2.0: This will essentially be a reference assembly. There will be no constructors for the types as ASP.NET Core will construct them based on their `HttpContext` and on framework there are already other constructors. However, this will allow class libraries to target .NET Standard instead of needing to multi-target which will then require everything it depends on to multi-target.
- .NET Framework 4.7.2: This will type forward the adapter classes to `System.Web` so that they can be unified and enable libraries built against .NET Standard 2.0 to run on .NET Framework instances.

## Known Limitations

Below are some of the limitations of the APIs in the adapters. These are usually due to building off of types used in ASP.NET Core that cannot be fully implemented in ASP.NET Core. In the future, analyzers may be used to flag usage to recommend better patterns.

- A number of APIs in `System.Web.HttpContext` are exposed as `NameValueCollection` instances. In order to reduce copying, many of these are implemented on ASP.NET Core using the core containers. This makes it so that for many of these collections, `Get(int)` (and any API that requires that such as `.Keys` or `.GetEnumerator()`) are unavailable as most of the containers in ASP.NET Core (such as `IHeaderDictionary`) does not have the ability to index by position.

# Reporting security issues and bugs

Security issues and bugs should be reported privately, via email, to the Microsoft Security Response Center (MSRC) secure@microsoft.com. You should receive a response within 24 hours. If for some reason you do not, please follow up via email to ensure we received your original message. Further information, including the MSRC PGP key, can be found in the [Security TechCenter](https://technet.microsoft.com/en-us/security/ff852094.aspx).

# Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md)

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow Microsoft's Trademark & Brand Guidelines. Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.

## Code of conduct

See [CODE-OF-CONDUCT](./CODE-OF-CONDUCT.md)
