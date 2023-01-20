using Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// An implementation of <see cref="ISkipEndpointSelector"/> that determines if an endpoint should be skipped.
/// </summary>
internal class QuerySkippableEndpointSelector : ISkippableEndpointSelector
{
    public ValueTask<bool> ShouldSkipAsync(HttpContext context)
    {
        if (context.Request.Query.TryGetValue("skip", out var existing) && existing is { Count: 1 } && bool.TryParse(existing[0], out var skip) && skip)
        {
            return ValueTask.FromResult(true);
        }

        return ValueTask.FromResult(false);
    }
}
