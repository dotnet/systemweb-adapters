using System.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace MvcApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            HttpContext.GetRequiredService<IServiceScopeFactory>();
            System.Web.HttpContext.Current.GetRequiredService<IServiceScopeFactory>();
            Session.Add("test-value", 5);
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
