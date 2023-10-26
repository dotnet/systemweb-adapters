# System.Web.Security.MachineKey Usage

The adapters expose the `System.Web.Security.MachineKey` APIs to protect and unprotect data. On ASP.NET Framework, these unify with the in-box versions as usual. However, on ASP.NET Core framework, it uses the new [Data Protection APIs].

## ASP.NET Framework Apps

In order to unprotect values from ASP.NET Core apps, .NET Framework apps must configure the app to [replace](https://learn.microsoft.com/aspnet/core/security/data-protection/compatibility/replacing-machinekey) the machine key implementations with data protection APIs. Once this is done, the ASP.NET Framework app will be using the same data protection APIs internally that ASP.NET Core is.

> NOTE: This will cause the framework application to be unable to unencrypt values previously encrypted. If needed, these values must be migrated in some way to use the data protection APIs. This is out of scope for this project, but should be considered when configuring this.
