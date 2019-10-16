using System.Web.Helpers;
using Moq;
using NUnit.Framework;
using System.Xml.XPath;
using TaskRouter.Web.Controllers;
using TaskRouter.Web.Infrastructure;
using TestStack.FluentMVCTesting;
using Twilio.Rest.Taskrouter.V1.Workspace;
using TaskRouter.Web.Tests.Extensions;

namespace TaskRouter.Web.Tests.Controllers
{
    public class MessageControllerTest
    {

        Mock<MessageController> mockMessageController()
        {
            var mockConfig = new Mock<Config>();
            mockConfig.SetupGet(x => x.AccountSID).Returns("ACXXXX...");
            mockConfig.SetupGet(x => x.AuthToken).Returns("auth tocken");

            var controller = new Mock<MessageController>(mockConfig.Object) { CallBase = true };

            controller
                .Setup(c => c.FetchWorker(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(WorkerResource.FromJson(Json.Encode(new {Sid = "WKXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" })));

            controller
                .Setup(c => c.UpdateWorker(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(WorkerResource.FromJson(Json.Encode(new { Sid = "WKXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" })));

            return controller;
        }

        [SetUp]
        public void SetUp()
        {
            Singleton.Instance.Workers["worker-phone-number"] = "WKXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
        }

        [TestCase("on", "online")]
        [TestCase("off", "offline")]
        [TestCase("invalid-input", "Unrecognized command")]
        public void Incoming_RespondsWithMessage(string body, string expectedMessage)
        {
            var controllerMock = mockMessageController();
            var controller = controllerMock.Object;
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
            var controllerMock = mockMessageController();
            var controller = controllerMock.Object;
            controller.Incoming("worker-phone-number", body);

            controllerMock.Verify(c => c.UpdateWorker(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Exactly(times));
        }
    }
}
