using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication
{
    public interface IAuthenticationService<T>
    {
        void Initialize(AuthenticationScheme scheme);

        Task<T> AuthenticateAsync(HttpRequest originalRequest, CancellationToken cancellationToken);
    }
}
