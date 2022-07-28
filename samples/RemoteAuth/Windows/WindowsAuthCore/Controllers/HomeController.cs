using Microsoft.AspNetCore.Mvc;

namespace WindowsAuthCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
