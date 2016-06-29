using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using TaskRouter.Web.Controllers;
using TaskRouter.Web.Models;
using TaskRouter.Web.Services;
using TestStack.FluentMVCTesting;

namespace TaskRouter.Web.Tests.Controllers
{
    public class HomeControllerTest
    {
        [Test]
        public void Index_RespondsWithDefaultView()
        {
            var mockService = new Mock<IMissedCallsService>();
            mockService.Setup(s => s.FindAllAsync()).ReturnsAsync(new List<MissedCall>());

            var controller = new HomeController(mockService.Object);
            controller.WithCallTo(c => c.Index())
                .ShouldRenderDefaultView();
        }
    }
}
