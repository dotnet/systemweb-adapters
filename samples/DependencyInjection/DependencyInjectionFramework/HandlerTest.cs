using System.Text.Json;
using System.Web;
using MvcApp;

namespace DependencyInjectionFramework
{
    public class HandlerTest : IHttpHandler
    {
        private readonly TransientService _transient1;
        private readonly TransientService _transient2;
        private readonly SingletonService _singleton;

        // NOTE: Handlers do not support scoped services
        public HandlerTest(SingletonService singleton, TransientService transient1, TransientService transient2)
        {
            _singleton = singleton;
            _transient1 = transient1;
            _transient2 = transient2;
        }

        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Write(TestService.IsValid(_singleton, _transient1, _transient2));
        }
    }
}
