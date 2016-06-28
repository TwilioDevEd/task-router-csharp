using Moq;
using NUnit.Framework;
using System.Xml.XPath;
using TaskRouter.Web.Controllers;
using TaskRouter.Web.Infrastructure;
using TaskRouter.Web.Tests.Extensions;
using TestStack.FluentMVCTesting;
using Twilio.TaskRouter;

namespace TaskRouter.Web.Tests.Controllers
{
    public class MessageControllerTest
    {
        private Mock<TaskRouterClient> _mockClient;

        [SetUp]
        public void SetUp()
        {
            _mockClient = new Mock<TaskRouterClient>(string.Empty, string.Empty);
            _mockClient.Setup(c => c.GetWorker(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Worker());

            Singleton.Instance.Workers["worker-phone-number"] = "WKXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
        }
        [TestCase("on", "online")]
        [TestCase("off", "offline")]
        [TestCase("invalid-input", "Unrecognized command")]
        public void Incoming_RespondsWithMessage(string body, string expectedMessage)
        {

            var controller = new MessageController(_mockClient.Object);
            controller.WithCallTo(c => c.Incoming("worker-phone-number", body))
                .ShouldReturnTwiMLResult(data =>
                 {
                     StringAssert.Contains(
                         expectedMessage, data.XPathSelectElement("Response/Message").Value);
                 });
        }

        [TestCase("on", 1)]
        [TestCase("off", 1)]
        [TestCase("invalid-input", 0)]
        public void Incoming_UpdatesWorkerAppropriately(string body, int times)
        {
            var controller = new MessageController(_mockClient.Object);
            controller.Incoming("worker-phone-number", body);

            _mockClient.Verify(c => c.UpdateWorker(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Exactly(times));
        }
    }
}
