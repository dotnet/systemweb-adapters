#if NET

namespace Microsoft.AspNetCore.Http;

public static class AspireRemoteAppHttpContextExtensions
{
    public static bool IsHandledRemotely(this HttpContext context)
        => context.GetEndpoint()?.Metadata.GetMetadata<FallbackMetadata>() is { };
}
#endif
