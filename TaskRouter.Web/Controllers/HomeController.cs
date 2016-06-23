using System.Web.Mvc;
using TaskRouter.Web.Domain;

namespace TaskRouter.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}