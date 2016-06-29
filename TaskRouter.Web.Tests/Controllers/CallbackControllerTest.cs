using NUnit.Framework;
using TaskRouter.Web.Controllers;
using TestStack.FluentMVCTesting;

namespace TaskRouter.Web.Tests.Controllers
{
    public class CallbackControllerTest
    {
        [Test]
        public void Assignment_RespondsWithDequeue()
        {
            var controller = new CallbackController();
            controller.WithCallTo(c => c.Assignment())
                .ShouldReturnJson(data =>
                 {
                     Assert.That(data.instruction, Is.EqualTo("dequeue"));
                 });
        }
    }
}
