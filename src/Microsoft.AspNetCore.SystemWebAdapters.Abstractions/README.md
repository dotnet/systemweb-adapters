# Microsoft.AspNetCore.SystemWebAdapters.Abstractions

Provides shared abstractions and interfaces for configuring System Web Adapters in both ASP.NET Core and ASP.NET Framework applications.

## Getting Started

This package is typically used as a dependency by other System Web Adapters packages. Most applications don't need to reference it directly.

## Usage

### Building Custom Extensions

Use this package when building extension libraries:

```csharp
using Microsoft.AspNetCore.SystemWebAdapters;

public static class MyAdapterExtensions
{
    public static ISystemWebAdapterBuilder AddMyCustomFeature(
        this ISystemWebAdapterBuilder builder)
    {
        builder.Services.AddSingleton<IMyService, MyService>();
        return builder;
    }
}
```

### Custom Session Serialization

Implement custom serializers for session state:

```csharp
using Microsoft.AspNetCore.SystemWebAdapters.SessionState;

public class MySessionSerializer : ISessionKeySerializer
{
    public bool TrySerialize(string key, object? obj, out byte[] bytes)
    {
        // Implement serialization logic
    }
    
    public bool TryDeserialize(string key, byte[] bytes, out object? obj)
    {
        // Implement deserialization logic
    }
}
```

## Additional Documentation

- [Incremental Migration Overview](https://learn.microsoft.com/aspnet/core/migration/inc/overview)
- [GitHub Repository](https://github.com/dotnet/systemweb-adapters)
