# Session State

Session state in ASP.NET Framework provided a number of features that ASP.NET Core does not provide. In order to migrate from ASP.NET Framework to Core, the adapters provide mechanisms to enable populating session state with similar behavior as `System.Web` did. Some of the differences between framework and core are:

- ASP.NET Framework would lock session usage within a session, so subsequent requests in a session are handled in a serial fashion. This is different than ASP.NET Core that does not provide any of these guarantees.
- ASP.NET Framework would serialize and deserialize objects automatically (unless being done in-memory). ASP.NET Core simply provides a mechanism to store a `byte[]` given a key. Any object serialization/deserialization has to be done manually be the user.

The adapter infrastructure exposes two interfaces that can be used to implement any session storage system. These are:

- `Microsoft.AspNetCore.SystemWebAdapters.ISessionManager`: This has a single method that gets passed an `HttpContext` and the session metadata and expects an `ISessionState` object to be returned.
- `Microsoft.AspNetCore.SystemWebAdapters.ISessionState`: This describes the state of a session object. It is used as the backing of the `System.Web.SessionState.HttpSessionState` type.

## Serialization
Since the adapters provide the ability to work with strongly-typed session state, we must be able to serialize and deserialize types. This is customized through the `Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization.ISessionKeySerializer`.

A default JSON implementation is provided that is configured via the `JsonSessionSerializerOptions`:

- `RegisterKey<T>(string)` - Registers a session key to a known type. This is required in order to serialize/deserialize the session state correctly. If a key is found that there is no registration for, an error will be thrown and session will not be available.

To use the default JSON backed implementation, add a package reference to [Microsoft.AspNetCore.SystemWebAdapters.SessionState](https://www.nuget.org/packages/Microsoft.AspNetCore.SystemWebAdapters.SessionState) and add the following to the startup:

> The Microsoft.AspNetCore.SystemWebAdapters.SessionState package is currently in preview. Remember to check the box to 'include prerelease' packages.

```csharp
builder.Services.AddSystemWebAdapters()
    .AddJsonSessionSerializer(options =>
    {
        // Serialization/deserialization requires each session key to be registered to a type
        options.RegisterKey<int>("test-value");
    });
```

## Implementations

There are two available implementations of the session state object that currently ship, each with some trade offs of features. The best choice for an application may depend on which part of the migration it is in, and may change over time.

- Strongly typed: Provides the ability to access an object and can be cast to the expected type
- Locking: Ensures multiple requests within a single session are queued up and aren't accessing the session at the same time
- Standalone: Use when you're not sharing session between ASP.NET Framework and ASP.NET Core to avoid modifying code in class libraries that references SessionState

Below are the available implementations:

| Implementation                                              | Strongly typed | Locking | Standalone |
|-------------------------------------------------------------|----------------|---------|------------|
| [Remote app](remote-session.md)                             | ✔️             | ✔️     | ⛔        |
| [Wrapped ASP.NET Core](wrapped-aspnetcore-session.md)       | ✔️             | ⛔     | ✔️        |
