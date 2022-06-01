# Remote Authentication

The System.Web adapter's remote authentication feature allows an ASP.NET Core app to determine a user's identity (authenticate an HTTP request) by deferring to an ASP.NET app. Enabling the feature adds an endpoint to the ASP.NET app that returns a serialized `ClaimsPrincipal` representing the authenticated user for any requests made to the endpoint. The ASP.NET Core app, then, registers a custom authentication handler that will (for endpoints with remote authentication enabled) determine a user's identity by calling that endpoint on the ASP.NET app and passing selected headers and cookies from the original request received by the ASP.NET Core app.

## Configuration

There are just a few small code changes need to enable remote authentication in a solution that's already using System.Web adapters and has an ASP.NET Core app (with migrated endpoints) proxying requests to an existing ASP.NET app (when the requests go to endpoints that aren't migrated yet).

### ASP.NET app configuration

First, the ASP.NET app needs configured to add the authentication endpoint. This is done by calling the `AddRemoteAuthentication` extension method on the `ISystemWebAdapterBuilder`:

```CSharp
Application.AddSystemWebAdapters()
    .AddRemoteAuthentication(options =>
    {
        options.RemoteServiceOptions.ApiKey = "MySecretKey";
    });
```

In the options configuration method passed to the `AddRemoteAuthentication` call, you must specify an API key which is used to secure the endpoint so that only trusted callers can make requests to it (this same API key will be provided to the ASP.NET Core app when it is configured). In addition to setting the API key, these options can also be used to specify the path for the authenticate endpoint (defaults to `/systemweb-adapters/authenticate`).

### ASP.NET Core app configuration

Next, the ASP.NET Core app needs configured to enable the authentication handler that will authenticate users by making an HTTP request to the ASP.NET app. This is done by calling `AddRemoteAppAuthentication` when registering System.Web adapters services:

```CSharp
builder.Services.AddSystemWebAdapters()
    .AddRemoteAppAuthentication(options =>
    {
        options.RemoteServiceOptions.RemoteAppUrl = new("http://URL-for-the-ASPNet-app");
        options.RemoteServiceOptions.ApiKey = "MySecretKey";
    });
```

In addition to configuring the remote app's URL and the shared secret ApiKey, the callback passed to `AddRemoteAppAuthentication` can also optionally specify some aspects of the remote authentication process's behavior:

* `RequestHeadersToForward`: This property contains headers that should be forwarded from a request when calling the authenticate API. By default, the only header forwarded is `Authorization`. If no headers are specified (including removing the default one), then all headers will be forwarded.
* `CookiesToForward`: This property contains cookies that should be forwarded from a request when calling the authenticate API. By default, all cookies are forwarded, but adding cookie names to this list will cause only those cookies to be used.
* `ResponseHeadersToForward`: This property lists response headers that should be propagated back from the Authenticate request to the original call that prompted authentication in scenarios where identity is challenged. By default, this includes `Location`, `Set-Cookie`, and `WWW-Authenticate` headers.
* `AuthenticationEndpointPath`: The endpoint on the ASP.NET app where authenticate requests should be made. This defaults to `/systemweb-adapters/authenticate` and must match the endpoint specified in ASP.NET authentication endpoint configuration.

In addition to these options, `RemoteAuthenticationOptions` derives from `AuthenticationSchemeOptions`, so it can be used to optionally specify authentication schemes to forward to for different authentication actions.

In addition to configuring services for the remote authentication handler, **endpoints must opt in to using remote authentication**. This can be done for an entire set of endpoints at once by calling `RequireRemoteAppAuthentication` while configuring the endpoints:

```CSharp
app.UseEndpoints(endpoints =>
{
    app.MapDefaultControllerRoute()
       .RequireRemoteAppAuthentication();

    app.MapReverseProxy();
});
```

Alternatively, the `[RemoteAuthentication]` attribute can be applied to specific controller classes or action methods to enable remote authentication just for those endpoints.

Finally, if the ASP.NET Core app didn't previously include authentication middleware, that will need to be enabled (after routing middleware, but before authorization middleware):

```CSharp
app.UseAuthentication();
```

## Design

1. When requests are processed by the ASP.NET Core app, the `RemoteAuthenticationAuthHandler` will attempt to authenticate the user for the request.
    1. The handler will first check whether the endpoint the request is being routed to has remote authentication metadata enabled (via a `[RemoteAuthentication]` or a call to `RequireRemoteAuthentication`). If remote authentication is not enabled, the handler will exit with a result of `NoResult`.
    1. If remote authentication is enabled for the endpoint, the handler will make an HTTP request to the ASP.NET app's authenticate endpoint. It will copy configured headers and cookies from the current request onto this new one in order to forward auth-relevant data. As mentioned above, default behavior is to copy the `Authorize` header and all cookies. The API key header is also added for security purposes.
1. The ASP.NET app's `RemoteAuthenticationHttpHandler` will serve requests sent to the authenticate endpoint. As long as the API keys match, the handler will return either the current user's ClaimsPrincipal serialized into the response body (the use should have already been logged in thanks to the forwarded headers and cookies) or it will return 401. ASP.NET authentication middleware will intercept the 401 response and either add relevant response headers (`WWW-Authenticate`, for example) or change the response to a 302 with a `Location` header to indicate where the requester should redirect to.
1. When the ASP.NET Core app's `RemoteAuthenticationAuthHandler` receives the response from the ASP.NET app.
    1. If a ClaimsPrincipal was successfully returned, the auth handler will deserialize it and use it as the current user's identity.
    1. If a ClaimsPrincipal was not successfully returned, the handler will store the result and if authentication is challenged (because the user is accessing a protected resource, for example), the request's response will be updated with the status code and selected response headers from the response from the authenticate endpoint. This enables challenge responses (like redirects to a login page) to be propagated to end users.
        1. Because results from the ASP.NET app's authenticate endpoint may include data specific to that endpoint, users can register `IRemoteAuthenticationResultProcessor` implementations with the ASP.NET Core app which will run on any authentication results before they are used. As an example, the one built-in `IRemoteAuthenticationResultProcessor` is `RedirectUrlProcessor` which looks for Location response headers returned from the authenticate endpoint and re-writes them so that instead of redirecting the user back to the authenticate endpoint after logging in, the header will instead redirect the user back to the URL of their original request after logging in.

## Known limitations

This remote authentication approach has a couple known limitations:

1. Becausae Windows authentication depends on a handle to a Windows identity, Windows authentication is not supported by this feature. Future work is planned to explore how shared Windows authentication might work.
1. This feature allows the ASP.NET Core app to make use of an identity authenticated by the ASP.NET app but all actions related to users (logging on, logging off, etc.) still need to be routed through the ASP.NET app.
