using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskRouter.Web.Controllers;
using TestStack.FluentMVCTesting;

namespace TaskRouter.Web.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            var controller = new HomeController();
            controller.WithCallTo(c => c.Index())
                .ShouldRenderDefaultView();
        }
    }
}
