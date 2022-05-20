# Remote App Session State

Remote app session state will enable communication between the ASP.NET Core and ASP.NET app and to retrieve the session state. This is enabled by exposing an endpoint on the ASP.NET app that can be queried to retrieve and set the session state.

## Configuration

In order to configure it, both the framework and core app must set an API key as well as register known app settings types. These properties are:

- `ApiKeyHeader` - header name that will contain an API key to secure the endpoint added on .NET Framework
- `ApiKey` - the shared API key that will be validated in the .NET Framework handler

Configuration for ASP.NET Core would look similar to the following:

```csharp
builder.Services.AddSystemWebAdapters()
    .AddJsonSessionSerializer(options =>
    {
        // Serialization/deserialization requires each session key to be registered to a type
        options.RegisterKey<int>("test-value");
        options.RegisterKey<SessionDemoModel>("SampleSessionItem");
    })
    .AddRemoteAppSession(options =>
    {
        // Provide the URL for the remote app that has enabled session querying
        options.RemoteApp = new(builder.Configuration["ReverseProxy:Clusters:fallbackCluster:Destinations:fallbackApp:Address"]);

        // Provide a strong API key that will be used to authenticate the request on the remote app for querying the session
        options.ApiKey = "strong-api-key";
    });
```

The framework equivalent would look like the following change in `Global.asax.cs`:

```csharp
Application.AddSystemWebAdapters()
    .AddRemoteAppSession(
        // Provide a strong API key that will be used to authenticate the request on the remote app for querying the session
        options => options.ApiKey = "strong-api-key",
        options =>
        {
            // Serialization/deserialization requires each session key to be registered to a type
            options.RegisterKey<int>("test-value");
            options.RegisterKey<SessionDemoModel>("SampleSessionItem");
        });
```
# Protocol

## Readonly
Readonly session will retrieve the session state from the framework app without any sort of locking. This consists of a single `GET` request that will return a session state and can be closed immediately.

```mermaid
    sequenceDiagram
        participant core as ASP.NET Core
        participant framework as ASP.NET
        participant session as Session Store
        core ->> framework: GET /session
        framework ->> session: Request session
        session -->> framework: Session
        framework -->> core: Session
```

## Writeable

Writeable session state protocol starts with the the same as the readonly, but differs in the following:

- Requires an additional `PUT` request to update the state
- The initial `GET` request must be kept open until the session is done; if closed, the session will not be able to be updated

```mermaid
    sequenceDiagram
        participant core as ASP.NET Core
        participant framework as ASP.NET
        participant session as Session Store
        core ->> framework: GET /session
        framework ->> session: Request session
        session -->> framework: Session
        framework -->> core: Session
        core ->> framework: PUT /session
        framework ->> framework: Deserialize to HttpSessionState
        framework -->> core: Session complete
        framework ->> session: Persist
```
