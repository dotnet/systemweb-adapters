using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace RemoteOAuth.Controllers
{
    public class AspNetValuesController : ApiController
    {
        // GET api/values
        [Route("/framework")]
        [Authorize]
        public object Get() => User is ClaimsPrincipal user ? new
        {
            Name = user.Identity?.Name,
            Claims = user.Claims.Select(c => new
            {
                c.Type,
                c.Value,
            })
        } : null;
    }
}
