using System.IO;
using System.Security.Claims;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// Interface for serializing and deserializing a ClaimsPrincipal to a stream.
/// </summary>
public interface IClaimsSerializer
{
    /// <summary>
    /// Deserializes a ClaimsPrincipal object from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize the ClaimsPrincipal from.</param>
    /// <returns>The ClaimsPrincipal deserialized from the input stream.</returns>
    ClaimsPrincipal Deserialize(Stream stream);

    /// <summary>
    /// Serializes a ClaimsPrincipal to a stream.
    /// </summary>
    /// <param name="claimsPrincipal">The ClaimsPrincipal to be serialized.</param>
    /// <param name="stream">The stream to write the serialized ClaimsPrincipal to.</param>
    void Serialize(ClaimsPrincipal claimsPrincipal, Stream stream);
}
