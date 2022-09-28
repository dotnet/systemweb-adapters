using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

internal class BinaryClaimsSerializer : IClaimsSerializer
{
    public ClaimsPrincipal? Deserialize(Stream responseContent)
    {
        if (responseContent == null)
        {
            return null;
        }
        using var reader = new BinaryReader(responseContent);
        return new ClaimsPrincipal(reader);
    }

    public void Serialize(ClaimsPrincipal claimsPrincipal, Stream outputStream)
    {
        if (claimsPrincipal == null)
        {
            throw new ArgumentNullException(nameof(claimsPrincipal));
        }

        if (outputStream == null)
        {
            throw new ArgumentNullException(nameof(outputStream));
        }

        using var writer = new BinaryWriter(outputStream);
        claimsPrincipal.WriteTo(writer);
    }
}

