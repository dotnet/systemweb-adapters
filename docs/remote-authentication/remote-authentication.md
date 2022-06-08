# Remote Authentication

The System.Web adapter's remote authentication feature allows an ASP.NET Core app to determine a user's identity (authenticate an HTTP request) by deferring to an ASP.NET app. Enabling the feature adds an endpoint to the ASP.NET app that returns a serialized `ClaimsPrincipal` representing the authenticated user for any requests made to the endpoint. The ASP.NET Core app, then, registers a custom authentication handler that will (for endpoints with remote authentication enabled) determine a user's identity by calling that endpoint on the ASP.NET app and passing selected headers and cookies from the original request received by the ASP.NET Core app.

## Configuration

There are just a few small code changes needed to enable remote authentication in a solution that's already set up according to the [Getting Started](../getting_started.md).

### ASP.NET app configuration

First, the ASP.NET app needs to be configured to add the authentication endpoint. This is done by calling the `AddRemoteAuthentication` extension method on the `ISystemWebAdapterBuilder`:

```CSharp
Application.AddSystemWebAdapters()
    .AddRemoteAuthentication(true, options =>
    {
        options.RemoteServiceOptions.ApiKey = "MySecretKey";
    });
```

The boolean that is passed to the `AddRemoteAuthentication` call specifies whether remote app authentication should be the default authentication scheme. Passing `true` will cause the user to be authenticated via remote app authentication for all requests, whereas passing `false` means that the user will only be authenticated with remote app authentication if the remote app scheme is specifically requested (with `[Authorize(AuthenticationSchemes = RemoteAppAuthenticationDefaults.AuthenticationScheme)]` on a controller or action method, for example). Passing false for this parameter has the advantage of only making HTTP requests to the original ASP.NET app for authentication for endpoints that require remote app authentication but has the disadvantage of requiring annotating all such endpoints to indicate that they will use remote app auth.

In the options configuration method passed to the `AddRemoteAuthentication` call, you must specify an API key which is used to secure the endpoint so that only trusted callers can make requests to it (this same API key will be provided to the ASP.NET Core app when it is configured). In addition to setting the API key, these options can also be used to specify the path for the authenticate endpoint (defaults to `/systemweb-adapters/authenticate`).

### ASP.NET Core app configuration

Next, the ASP.NET Core app needs to be configured to enable the authentication handler that will authenticate users by making an HTTP request to the ASP.NET app. This is done by calling `AddRemoteAppAuthentication` when registering System.Web adapters services:

```CSharp
builder.Services.AddSystemWebAdapters()
    .AddRemoteAppAuthentication(options =>
    {
        options.RemoteServiceOptions.RemoteAppUrl = new("http://URL-for-the-ASPNet-app");
        options.RemoteServiceOptions.ApiKey = "MySecretKey";
    });
```

In addition to configuring the remote app's URL and the shared secret API key, the callback passed to `AddRemoteAppAuthentication` can also optionally specify some aspects of the remote authentication process's behavior:

* `RequestHeadersToForward`: This property contains headers that should be forwarded from a request when calling the authenticate API. By default, the only headers forwarded are `Authorization` and `Cookie`. Additional headers can be forwarded by adding them to this list. Alternatively, if the list is cleared (so that no headers are specified), then all headers will be forwarded.
* `ResponseHeadersToForward`: This property lists response headers that should be propagated back from the authenticate request to the original call that prompted authentication in scenarios where identity is challenged. By default, this includes `Location`, `Set-Cookie`, and `WWW-Authenticate` headers.
* `AuthenticationEndpointPath`: The endpoint on the ASP.NET app where authenticate requests should be made. This defaults to `/systemweb-adapters/authenticate` and must match the endpoint specified in the ASP.NET authentication endpoint configuration.

In addition to these options, `RemoteAuthenticationOptions` derives from `AuthenticationSchemeOptions`, so it can be used to optionally specify authentication schemes to forward to for different authentication actions.

Finally, if the ASP.NET Core app didn't previously include authentication middleware, that will need to be enabled (after routing middleware, but before authorization middleware):

```CSharp
app.UseAuthentication();
```

## Design

1. When requests are processed by the ASP.NET Core app, the `RemoteAuthenticationAuthHandler` will attempt to authenticate the user for the request.
    1. The handler will first check whether the endpoint the request is being routed to has remote authentication metadata enabled (via a `[RemoteAuthentication]` or a call to `RequireRemoteAuthentication`). If remote authentication is not enabled, the handler will exit with a result of `NoResult`.
    1. If remote authentication is enabled for the endpoint, the handler will make an HTTP request to the ASP.NET app's authenticate endpoint. It will copy configured headers and cookies from the current request onto this new one in order to forward auth-relevant data. As mentioned above, default behavior is to copy the `Authorize` header and all cookies. The API key header is also added for security purposes.
1. The ASP.NET app will serve requests sent to the authenticate endpoint. As long as the API keys match, the ASP.NET app will return either the current user's ClaimsPrincipal serialized into the response body or it will return an HTTP status code (like 401 or 302) and response headers indicating failure.
1. When the ASP.NET Core app's `RemoteAuthenticationAuthHandler` receives the response from the ASP.NET app.
    1. If a ClaimsPrincipal was successfully returned, the auth handler will deserialize it and use it as the current user's identity.
    1. If a ClaimsPrincipal was not successfully returned, the handler will store the result and if authentication is challenged (because the user is accessing a protected resource, for example), the request's response will be updated with the status code and selected response headers from the response from the authenticate endpoint. This enables challenge responses (like redirects to a login page) to be propagated to end users.
        1. Because results from the ASP.NET app's authenticate endpoint may include data specific to that endpoint, users can register `IRemoteAuthenticationResultProcessor` implementations with the ASP.NET Core app which will run on any authentication results before they are used. As an example, the one built-in `IRemoteAuthenticationResultProcessor` is `RedirectUrlProcessor` which looks for Location response headers returned from the authenticate endpoint and re-writes them so that instead of redirecting the user back to the authenticate endpoint after logging in, the header will instead redirect the user back to the URL of their original request after logging in.

## Known limitations

This remote authentication approach has a couple known limitations:

1. Because Windows authentication depends on a handle to a Windows identity, Windows authentication is not supported by this feature. Future work is planned to explore how shared Windows authentication might work.
1. This feature allows the ASP.NET Core app to make use of an identity authenticated by the ASP.NET app but all actions related to users (logging on, logging off, etc.) still need to be routed through the ASP.NET app.
