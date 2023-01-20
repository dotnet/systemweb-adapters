using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// An interface that determines if an endpoint should be skipped
/// </summary>
public interface ISkippableEndpointSelector
{
    ValueTask<bool> ShouldSkipAsync(HttpContext context);
}
