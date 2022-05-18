using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public interface IAuthenticationResultFactory<T>
{
    Task<T> CreateRemoteAuthenticationResultAsync(HttpResponseMessage response, RemoteAuthenticationOptions options);
}
