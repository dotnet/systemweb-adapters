#if NET

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.Routing;

public static class AspireRemoteAppExtensions
{
    public static IEndpointConventionBuilder MapRemoteAppFallback(this IEndpointRouteBuilder app, [StringSyntax("Route")] string? pattern = "/{**catch-all}")
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(pattern);

        var url = app.ServiceProvider.GetRequiredService<IOptions<RemoteAppClientOptions>>().Value.RemoteAppUrl.OriginalString;

        return app.MapForwarder(pattern, url)

            // If there is a route locally, we want to ensure that is used by default, but otherwise we'll forward
            .WithOrder(int.MaxValue)

            // We want to mark this endpoint as a fallback for remote requests in case we need to identify it later
            .WithMetadata(FallbackMetadata.Instance);
    }

    public static IApplicationBuilder UseWhenLocal(this IApplicationBuilder builder, Action<IApplicationBuilder> configuration)
        => builder.UseWhen(static context => !context.IsHandledRemotely(), configuration);

    public static IApplicationBuilder UseWhenRemote(this IApplicationBuilder builder, Action<IApplicationBuilder> configuration)
        => builder.UseWhen(static context => context.IsHandledRemotely(), configuration);
}
#endif
