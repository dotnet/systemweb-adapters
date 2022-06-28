# Usage Guidance

`Microsoft.AspNetCore.SystemWebAdapters` provides an emulation layer to mimic behavior from ASP.NET framework on ASP.NET Core. Below are some guidelines for some of the considerations when using them:

## `HttpContext` lifetime

The adapters are backed by `Microsoft.AspNetCore.Http.HttpContext` which cannot be used past the lifetime of a request. Thus, `System.Web.HttpContext` when run on ASP.NET Core cannot be used past a request as well, while on ASP.NET Framework it would work at times. An `ObjectDisposedException` will be thrown in cases where it is used past a request end.

**Recommendation**: Store the values needed into a POCO and hold onto that.

## Conversion to `System.Web.HttpContext`

There are two ways to convert an `Microsoft.AspNetCore.Http.HttpContext` to a `System.Web.HttpContext`:

- Implicit casting
- Constructor usage

**Recommendation**: For the most cases, implicit casting should be preferred as this will cache the created instance and ensure only a single `System.Web.HttpContext` per request.

## `CultureInfo.CurrentCulture` is not set by default

In ASP.NET Framework, `CultureInfo.Current` was set for a request, but this is not done automatically in ASP.NET Core. Instead, you must add the appropriate middleware to your pipeline.

**Recommendation**: See [ASP.NET Core Localization](https://docs.microsoft.com/aspnet/core/fundamentals/localization#localization-middleware) for details on how to enable this.

Simplest way to enable this with similar behavior as ASP.NET Framework would be to add the following to your pipeline:

```csharp
app.UseRequestLocalization();
```

## Request thread does not exist in ASP.NET Core

In ASP.NET Framework, a request had thread-affinity and `HttpContext.Current` would only be available if on that thread. ASP.NET Core does not have this guarantee so `HttpContext.Current` will be available within the same async context, but no guarantees about threads are made.

**Recommendation**: If reading/writing to the `HttpContext`, you must ensure you are doing so in a single-threaded way.


## `HttpContext.Request` may need to be prebuffered

By default, the incoming request is not always seekable nor fully available. In order to get behavior seen in .NET Framework, you can opt into prebuffering the input stream. This will fully read the incoming stream and buffer it to memory or disk (depending on settings). 

**Recommendation**: This can be enabled by applying endpoint metadata that implements the `IPreBufferRequestStreamMetadata` interface. This is available as an attribute `PreBufferRequestStreamAttribute` that can be applied to controllers or methods.

To enable this on all MVC endpoints, there is an extension method that can be used as follows:

```cs
app.MapDefaultControllerRoute()
    .PreBufferRequestStream();
```

## `HttpContext.Response` may require buffering

Some APIs on `HttpContext.Response` require that the output stream is buffered, such as `HttpResponse.Output`, `HttpResponse.End()`, `HttpResponse.Clear()`, and `HttpResponse.SuppressContent`.

**Recommendation**: In order to support behavior for `HttpContext.Response` that requires buffering the response before sending, endpoints must opt-into it with endpoint metadata implementing `IBufferResponseStreamMetadata`.

To enable this on all MVC endpoints, there is an extension method that can be used as follows:

```cs
app.MapDefaultControllerRoute()
    .BufferResponseStream();
```

## Shared session state

In order to support `HttpContext.Session`, endpoints must opt-into it via metadata implementing `ISessionMetadata`.

**Recommendation**: To enable this on all MVC endpoints, there is an extension method that can be used as follows:

```cs
app.MapDefaultControllerRoute()
    .RequireSystemWebAdapterSession();
```

This also requires some implementation of a session store. For details of options here, see [here](./session-state/session.md).

## Remote session exposes additional endpoint for application

The [remote session support](session-state/remote-session.md) exposes an endpoint that allows the core app to retrieve session information. This may cause a potentially long-lived request to exist between the core app and the framework app, but will time out with the current request or the session timeout (by default is 20 minutes).

**Recommendation**: Ensure the API key used is a strong one and that the connection with the framework app is done over SSL.

