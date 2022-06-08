using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RemoteBearerCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AspNetCoreValuesController : ControllerBase
    {
        public AspNetCoreValuesController()
        {
        }

        [Authorize]
        [HttpGet]
        public IEnumerable<string?> Get()
        {
            yield return User.Identity?.Name;
            if (User is ClaimsPrincipal principal)
            {
                foreach (var claim in principal.Claims)
                {
                    yield return $"{claim.Type}: {claim.Value}";
                }
            }
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "Hello from ASP.NET Core";
        }
    }
}
