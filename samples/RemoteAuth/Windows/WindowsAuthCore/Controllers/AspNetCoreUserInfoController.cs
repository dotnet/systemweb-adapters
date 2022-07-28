using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WindowsAuthCore.Controllers
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
