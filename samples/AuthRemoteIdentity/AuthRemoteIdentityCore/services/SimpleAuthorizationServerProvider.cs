using System.Net.Sockets;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace AuthRemoteIdentityCore.Services;

public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
{

    public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
    {
        context.Validated();
        return Task.CompletedTask;
    }

    public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
    {
        var identity = new ClaimsIdentity(context.Options.AuthenticationType);

        var properties = new Dictionary<string, string>();
        var props = new AuthenticationProperties(properties);

        var ticket = new AuthenticationTicket(identity, props);
        context.Validated(ticket);
        var user = Thread.CurrentPrincipal;
        var user2 = ClaimsPrincipal.Current;

    }
}
