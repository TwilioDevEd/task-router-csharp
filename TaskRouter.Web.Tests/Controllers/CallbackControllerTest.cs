using NUnit.Framework;
using TaskRouter.Web.Infrastructure;
using TaskRouter.Web.Controllers;
using TestStack.FluentMVCTesting;
using Moq;

namespace TaskRouter.Web.Tests.Controllers
{
    public class CallbackControllerTest
    {
        [Test]
        public void Assignment_RespondsWithDequeue()
        {
            var mockConfig = new Mock<Config>();
            mockConfig.SetupGet(x => x.AccountSID).Returns("ACXXXXXX...");
            mockConfig.SetupGet(x => x.AuthToken).Returns("auth token");

            var controller = new CallbackController(mockConfig.Object);
            controller.WithCallTo(c => c.Assignment())
                .ShouldReturnJson(data =>
                 {
                     Assert.That(data.instruction, Is.EqualTo("dequeue"));
                 });
        }
    }
}
