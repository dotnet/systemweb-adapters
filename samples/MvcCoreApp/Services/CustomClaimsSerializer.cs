using System.Security.Claims;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication;

namespace MvcCoreApp.Services
{
    internal class CustomClaimsSerializer : IClaimsSerializer
    {
        private readonly ILogger<CustomClaimsSerializer> _logger;

        public CustomClaimsSerializer(ILogger<CustomClaimsSerializer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ClaimsPrincipal? Deserialize(Stream? responseContent)
        {
            _logger.LogInformation("Deserializing claims with customer serializer");
            if (responseContent == null)
            {
                return null;
            }
            using var reader = new BinaryReader(responseContent);
            return new ClaimsPrincipal(reader);
        }

        public void Serialize(ClaimsPrincipal? claimsPrincipal, Stream? outputStream)
        {
            throw new NotImplementedException();
        }
    }
}
