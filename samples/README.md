# Samples

This folder contains samples for migrating ASP.NET Framework to ASP.NET Core using the adapters in this repo. All of these apps share the `ClassLibrary` project that has shared System.Web API usage via the adapters.

- *`CoreApp`*: This app is an ASP.NET Core application only that is still using System.Web APIs running completely on ASP.NET Core
- *`RemoteApp/*`*: These apps are ASP.NET Framework and Core application pairs that showcases using remote app authentication in the ASP.NET Core application with different auth types:
  - *`RemoteApp/Bearer`*: This uses bearer authentication. Requires setting up an Azure B2C instance and filling in the values in `appsettings.json`
  - *`RemoteApp/Forms`*: This uses forms based authentication
  - *`RemoteApp/Identity`*: This uses ASP.NET Framework identity for authentication
- *[`MachineKey`](MachineKey/README.md)*: This app shows how to share `System.Web.Security.MachineKey` calls between framework and core
