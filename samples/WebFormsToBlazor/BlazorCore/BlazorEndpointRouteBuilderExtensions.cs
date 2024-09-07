using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace BlazorCore;

public static class BlazorEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapBlazorPages(this IEndpointRouteBuilder endpoints, string page)
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly is null)
        {
            throw new InvalidOperationException("No entry assembly available.");
        }
        return endpoints.MapBlazorPages(page, assembly);
    }

    public static IEndpointConventionBuilder MapBlazorPages(this IEndpointRouteBuilder endpoints, string page, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        var builder = new BlazorEndpointConventionBuilder();
        foreach (var route in GetRoutes(assemblies))
        {
            var conventionBuilder = endpoints.MapFallbackToPage(route, page);
            conventionBuilder.Add(b =>
            {
                // By default this will be 'Fallback {route}', but this will help identify it is explicitly registered
                b.DisplayName = $"Blazor {route}";
                // {page} will, by default, have Order = 0; this will ensure the order is not the same
                ((RouteEndpointBuilder)b).Order = -1;
            });
            builder.Add(conventionBuilder);
        }
        return builder;
    }

    private static IEnumerable<string> GetRoutes(Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IComponent).IsAssignableFrom(type))
                {
                    foreach (var attribute in type.GetCustomAttributes<RouteAttribute>())
                    {
                        if (attribute is RouteAttribute { Template: { } route })
                        {
                            yield return route;
                        }
                    }
                }
            }
        }
    }

    private sealed class BlazorEndpointConventionBuilder : IEndpointConventionBuilder
    {
        private readonly List<IEndpointConventionBuilder> _builders = new();
        public void Add(IEndpointConventionBuilder builder)
        {
            _builders.Add(builder);
        }
        void IEndpointConventionBuilder.Add(Action<EndpointBuilder> convention)
        {
            foreach (var builder in _builders)
            {
                builder.Add(convention);
            }
        }
#if NET7_0_OR_GREATER
        void IEndpointConventionBuilder.Finally(Action<EndpointBuilder> finalConvention)
        {
            foreach (var builder in _builders)
            {
                builder.Finally(finalConvention);
            }
        }
#endif
    }
}
