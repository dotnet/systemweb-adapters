using Microsoft.AspNetCore.SystemWebAdapters;

namespace MvcCoreApp;

public class QuerySkippableSelector : ISkippableEndpointSelector
{
    public ValueTask<bool> ShouldSkipAsync(HttpContext context)
    {
        var result = context.Request.Query.TryGetValue("skip", out var values) &&
            values is { Count: 1 } &&
            bool.TryParse(values[0], out var skip)
            && skip;

        return ValueTask.FromResult(result);
    }
}
