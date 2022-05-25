using System.Collections.Generic;
using System.Web.Http;

namespace RemoteOAuth.Controllers
{
    public class AspNetValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2", "Hello from ASP.NET" };
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
