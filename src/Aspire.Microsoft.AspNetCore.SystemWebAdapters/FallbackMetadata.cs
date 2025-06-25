#if NET

namespace Microsoft.AspNetCore.Http;

internal sealed class FallbackMetadata
{
    public static FallbackMetadata Instance { get; } = new();
}
#endif
