# System.Web adapters for ASP.NET Core

This project provides a collection of adapters that help migrating from `System.Web.dll` based ASP.NET projects to ASP.NET Core projects. The adapters currently include:

- `Microsoft.AspNetCore.SystemWebAdapters`: Subset of the APIs from `System.Web.dll` backed by `Microsoft.AspNetCore.Http` types
- `Microsoft.AspNetCore.SystemWebAdapters.CoreServices`: Support for adding services to ASP.NET Core applications to enable migration efforts
- `Microsoft.AspNetCore.SystemWebAdapters.FrameworkServices`: Support for adding services to ASP.NET Framework applications to enable migration efforts
- `Microsoft.AspNetCore.SystemWebAdapters.Abstractions`: A collection of abstractions shared between the ASP.NET Core and .NET Framework implementations, such as session serialization interfaces.

These adapters help enable large scale, incremental migration from ASP.NET to ASP.NET Core. For more details on incremental migration from ASP.NET to ASP.NET Core, please see the [documentation](docs).

## Get started

Use the [Getting Started](docs/getting_started.md) guide in the docs to start using the System.Web adapters as part of an incremental migration from ASP.NET to ASP.NET Core.

## Set up

Below are the steps needed to start using the System.Web adapters with your ASP.NET project:

1. *Optional for nightly adapter builds*: Set up `NuGet.config` to point to the CI feed:
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
2. Install `Microsoft.AspNetCore.SystemWebAdapters` to supporting libraries
   - Class libraries can target .NET Standard 2.0 if desired which will ensure you are using the shared surface area
   - If you find that there's still some missing APIs, you may cross-compile with .NET Framework to maintain that behavior and handle it in .NET core in some other way
   - There should be no manual changes to enable using supported surface area of the adapters. If a member is not found, it is not currently supported on ASP.NET Core
3. Install `Microsoft.AspNetCore.SystemWebAdapters.CoreServices` to your ASP.NET Core application
4. Install `Microsoft.AspNetCore.SystemWebAdapters.FrameworkServices` to your ASP.NET Framework application
   - The package installation will add a new module to your `web.config`. This module handles any customizations that are required to help migrate to .NET Core. See [this](docs/framework.md) for details on what is available here.
5. For your ASP.NET Core application:
   - Register the adapter services:
     ```csharp
     builder.Services.AddSystemWebAdapters();
     ```
   - Add the middleware after routing but before endpoints (if present);
     ```csharp
     app.UseSystemWebAdapters();
     ```
   - For additional configuration, please see the [configuration](./docs/core.md) section

## Supported Targets

- .NET 6.0: This will implement the adapters against ASP.NET Core `HttpContext`. This will provide the following:
  - Conversions between ASP.NET Core `HttpContext` and `System.Web` adapter `HttpContext` (with appropriate caching so it will not cause perf hits for GC allocations)
  - Default implementations against `Microsoft.AspNetCore.Http.HttpContext`
  - Services that can be implemented to override some functionality such as session/caching/etc that may need to be customized to match experience.
- .NET Standard 2.0: This will essentially be a reference assembly. There will be no constructors for the types as ASP.NET Core will construct them based on their `HttpContext` and on framework there are already other constructors. However, this will allow class libraries to target .NET Standard instead of needing to multi-target which will then require everything it depends on to multi-target.
- .NET Framework 4.7.2: This will type forward the adapter classes to `System.Web` so that they can be unified and enable libraries built against .NET Standard 2.0 to run on .NET Framework instances.

## Known Limitations

Below are some of the limitations of the APIs in the adapters. These are usually due to building off of types used in ASP.NET Core that cannot fully implement the shape of the API in  ASP.NET Framework. In the future, analyzers may be used to flag usage to recommend better patterns.

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
