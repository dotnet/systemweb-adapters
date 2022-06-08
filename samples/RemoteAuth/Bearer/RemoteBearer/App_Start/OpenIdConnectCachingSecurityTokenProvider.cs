using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Jwt;

// Adapted from https://github.com/Azure-Samples/active-directory-b2c-dotnet-webapp-and-webapi
// and https://github.com/aspnet/AspNetKatana/blob/main/src/Microsoft.Owin.Security.ActiveDirectory/WsFedCachingSecurityKeyProvider.cs

namespace RemoteOAuth
{
    // This class is necessary because the OAuthBearer Middleware does not leverage
    // the OpenID Connect metadata endpoint exposed by the security token service by default.
    public class OpenIdConnectCachingSecurityTokenProvider : IIssuerSecurityKeyProvider
    {
        private readonly TimeSpan _refreshInterval = new TimeSpan(1, 0, 0, 0);

        private DateTimeOffset _syncAfter = new DateTimeOffset(new DateTime(2001, 1, 1));
        public ConfigurationManager<OpenIdConnectConfiguration> _configManager;
        private string _issuer;
        private IEnumerable<SecurityKey> _keys;

        public OpenIdConnectCachingSecurityTokenProvider(string metadataEndpoint)
        {
            _configManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataEndpoint, new OpenIdConnectConfigurationRetriever());

            RetrieveMetadata();
        }

        /// <summary>
        /// Gets the issuer the credentials are for.
        /// </summary>
        /// <value>
        /// The issuer the credentials are for.
        /// </value>
        public string Issuer
        {
            get
            {
                RefreshMetadata();
                return _issuer;
            }
        }

        /// <summary>
        /// Gets all known security keys.
        /// </summary>
        /// <value>
        /// All known security keys.
        /// </value>
        public IEnumerable<SecurityKey> SecurityKeys
        {
            get
            {
                RefreshMetadata();
                return _keys;
            }
        }

        private void RefreshMetadata()
        {
            if (_syncAfter >= DateTimeOffset.UtcNow)
            {
                return;
            }

            // Queue a refresh, but discourage other threads from doing so.
            _syncAfter = DateTimeOffset.UtcNow + _refreshInterval;
            Task.Run(() => RetrieveMetadata());
        }


        private void RetrieveMetadata()
        {
            _syncAfter = DateTimeOffset.UtcNow + _refreshInterval;
            OpenIdConnectConfiguration config = Task.Run(_configManager.GetConfigurationAsync).Result;
            _issuer = config.Issuer;
            _keys = config.SigningKeys;
        }
    }
}
