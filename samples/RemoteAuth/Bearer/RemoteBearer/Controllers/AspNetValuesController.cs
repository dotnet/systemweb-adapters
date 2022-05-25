using System.Collections.Generic;
using System.Security.Claims;
using System.Web.Http;

namespace RemoteOAuth.Controllers
{
    public class AspNetValuesController : ApiController
    {
        // GET api/values
        [Authorize]
        public IEnumerable<string> Get()
        {
            yield return User.Identity.Name;
            if (User is ClaimsPrincipal principal)
            {
                foreach (var claim in principal.Claims)
                {
                    yield return $"{claim.Type}: {claim.Value}";
                }
            }
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "Hello from ASP.NET";
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
