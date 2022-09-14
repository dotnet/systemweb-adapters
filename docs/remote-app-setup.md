# Remote app setup

In some incremental upgrade scenarios, it's useful for the new ASP.NET Core app to be able to communicate with the original ASP.NET app.

Specifically, this capability is used, currently, for [remote app authentication](remote-authentication/remote-authentication.md) and [remote session](session-state/remote-session.md) features.

## Configuration

To enable the ASP.NET Core app to communicate with the ASP.NET app, it's necessary to make a couple small changes to each app.

### ASP.NET app configuration

To setup the ASP.NET app to be able to receive requests from the ASP.NET Core app, call the `AddRemoteApp` extension method on the `ISystemWebAdapterBuilder` as shown here.

```CSharp
SystemWebAdapterConfiguration.AddSystemWebAdapters(this)
    .AddRemoteAppServer(options =>
    {
        // ApiKey is a string representing a GUID
        options.ApiKey = ConfigurationManager.AppSettings["RemoteAppApiKey"];
    });
```

In the options configuration method passed to the `AddRemoteApp` call, you must specify an API key which is used to secure the endpoint so that only trusted callers can make requests to it (this same API key will be provided to the ASP.NET Core app when it is configured). The API key is a string and must be parsable as a GUID (128-bit hex number). Hyphens in the key are optional.

### ASP.NET Core app

To setup the ASP.NET Core app to be able to send requests to the ASP.NET app, you need to make a similar change, calling `AddRemoteApp` after registering System.Web adapter services with `AddSystemWebAdapters`.

```CSharp
builder.Services.AddSystemWebAdapters()
    .AddRemoteAppClient(options =>
    {
        options.RemoteAppUrl = new(builder.Configuration["ReverseProxy:Clusters:fallbackCluster:Destinations:fallbackApp:Address"]);
        options.ApiKey = builder.Configuration("RemoteAppApiKey");
    });
```

The `AddRemoteApp` call is used to configure the remote app's URL and the shared secret API key.

With both the ASP.NET and ASP.NET Core app updated, extension methods can now be used to setup [remote app authentication](remote-authentication/remote-authentication.md) or [remote session](session-state/remote-session.md), as needed.

## Securing the remote app connection

Because remote app features involve serving requests on new endpoints from the ASP.NET app, it's important that communication to and from the ASP.NET app be secure.

First, make sure that the API key string used to authenticate the ASP.NET Core app with the ASP.NET app is unique and kept secret. It is a best practice to not store the API key in source control. Instead, load it at runtime from a secure source such as Azure Key Vault or other secure runtime configuration. In order to encourage secure API keys, remote app connections require that the keys be non-empty GUIDs (128-bit hex numbers).

Second, because it's important for the ASP.NET Core app to be able to trust that it is requesting information from the correct ASP.NET app, the ASP.NET app should use HTTPS in any production scenarios so that the ASP.NET Core app can know responses are being served by a trusted source.
