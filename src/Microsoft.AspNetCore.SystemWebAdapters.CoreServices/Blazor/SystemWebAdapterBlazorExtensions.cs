using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters.Blazor;

internal static class SystemWebAdapterBlazorExtensions
{
    public static void AddBlazorSupport(this IServiceCollection services)
    {
        services.AddSingleton<CircuitHandler, HttpContextCircuitHandler>();
    }
}
