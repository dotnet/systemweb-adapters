using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public interface IClaimsSerializer
{
    ClaimsPrincipal? Deserialize(Stream? responseContent);
    void Serialize(ClaimsPrincipal? claimsPrincipal, Stream? outputStream);
}
