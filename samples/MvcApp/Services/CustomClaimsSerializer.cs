using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication;

namespace MvcApp.Services
{
    internal class CustomClaimsSerializer : IClaimsSerializer
    {
        public ClaimsPrincipal Deserialize(Stream responseContent)
        {
            throw new NotImplementedException();
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

            claimsPrincipal.Identities.FirstOrDefault()?.AddClaim(new Claim("CustomInjectedClaim", "Foo"));

            using (var writer = new BinaryWriter(outputStream))
            {
                claimsPrincipal.WriteTo(writer);
            }
        }
    }
}
