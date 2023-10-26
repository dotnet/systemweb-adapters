using System.Web.SessionState;
using ClassLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace MvcCoreApp.Controllers
{
    [PreBufferRequestStream]
    [BufferResponseStream]
    public class TestController : Controller
    {
        [HttpGet]
        [Session(SessionBehavior = SessionStateBehavior.ReadOnly)]
        [Route("/api/test/request/info")]
        public void Get([FromQuery] bool? suppress = false) => RequestInfo.WriteRequestInfo(suppress ?? false);

        [Route("/api/test/request/cookie")]
        public void TestRequestCookie() => CookieTests.RequestCookies(HttpContext);

        [Route("/api/test/response/cookie")]
        [HttpGet]
        public void TestResponseCookie(bool shareable = false)
        {
            // Force public cache control for testing Shareable behavior
            HttpContext.Response.Headers["Cache-Control"] = "public";

            CookieTests.ResponseCookies(HttpContext, shareable);
        }
    }
}
