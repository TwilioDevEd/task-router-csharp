using NUnit.Framework;
using TaskRouter.Web.Controllers;
using TestStack.FluentMVCTesting;

namespace TaskRouter.Web.Tests.Controllers
{
    public class HomeControllerTest
    {
        [Test]
        public void Index()
        {
            var controller = new HomeController();
            controller.WithCallTo(c => c.Index())
                .ShouldRenderDefaultView();
        }
    }
}
