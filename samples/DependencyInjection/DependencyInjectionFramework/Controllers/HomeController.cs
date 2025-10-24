using System.Web.Mvc;

namespace MvcApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly TransientService _transient1;
        private readonly TransientService _transient2;
        private readonly SingletonService _singleton;
        private readonly ScopedService _scoped1;
        private readonly ScopedService _scoped2;

        public HomeController(
            SingletonService singleton,
            ScopedService scoped1,
            ScopedService scoped2,
            TransientService transient1,
            TransientService transient2)
        {
            _singleton = singleton;
            _scoped1 = scoped1;
            _scoped2 = scoped2;
            _transient1 = transient1;
            _transient2 = transient2;
        }

        [Route("~/mvc")]
        public ActionResult Mvc()
        {
            ViewBag.Message = TestService.IsValid(_singleton, _scoped1, _scoped2, _transient1, _transient2);

            return View();
        }
    }
}
