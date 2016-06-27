using System.Threading.Tasks;
using System.Web.Mvc;
using TaskRouter.Web.Models;
using TaskRouter.Web.Services;

namespace TaskRouter.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMissedCallsService _service;

        public HomeController()
        {
            _service = new MissedCallsService(new TaskRouterDbContext());
        }

        public HomeController(IMissedCallsService service)
        {
            _service = service;
        }

        public async Task<ActionResult> Index()
        {
            var missedCalls = await _service.FindAllAsync();
            return View(missedCalls);
        }
    }
}