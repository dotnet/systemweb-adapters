# WebForms Migration to Blazor

Migrating WebForms to .NET Core is not a straightforward approach. This sample shows how the incremental migration can be used to start migrating to Blazor by using Blazor pages for completed routes and Blazor components in the WebForms project for controls.

Things to look at in this sample:

- [BlazorEndpointRouteBuilderExtensions](BlazorCore/BlazorEndpointRouteBuilderExtensions.cs): A set of helper methods to enable Blazor to live in the same project as YARP. See [documentation](https://learn.microsoft.com/en-us/aspnet/core/migration/inc/blazor) on how this works.
    > Note: By default, the Blazor project uses `_Host` with a page directive registering it as the root `/` page. If you want `/` to go to the backend app, you'll need to change this page directive to something else. In the sample it is changed to `/blazor-host`
- Using `RegisterCustomElement` to expose a Blazor component in the [ASP.NET Core Project](BlazorCore/Program.cs) that is consumed in the WebForms [SiteMaster](WebFormsFramework/Site.Master#58).
