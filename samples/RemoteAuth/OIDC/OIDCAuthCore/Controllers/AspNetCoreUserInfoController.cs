using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OIDCAuthCore.Controllers
{
    public class AspNetCoreUserInfoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Authorized()
        {
            return View("Index");
        }
    }
}
