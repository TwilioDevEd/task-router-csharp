using NUnit.Framework;
using System.Xml.XPath;
using FluentMvcTesting.Extensions;
using TaskRouter.Web.Controllers;
using TestStack.FluentMVCTesting;

namespace TaskRouter.Web.Tests.Controllers
{
    public class CallControllerTest
    {
        [Test]
        public void Incoming_RespondsWithWelcomeMessage()
        {
            var controller = new CallController();
            controller.WithCallTo(c => c.Incoming())
                .ShouldReturnXmlResult(data =>
                 {
                     StringAssert.Contains(
                         "For Programmable SMS", data.XPathSelectElement("Response/Gather/Say").Value);
                 });
        }

        [TestCase("1", "ProgrammableSMS")]
        [TestCase("2", "ProgrammableVoice")]
        public void Enqueue_EnqueuesTheSelectedProduct(string digits, string selectedProduct)
        {
            var controller = new CallController();
            controller.WithCallTo(c => c.Enqueue(digits))
                .ShouldReturnXmlResult(data =>
                 {
                     StringAssert.Contains(
                         selectedProduct, data.XPathSelectElement("Response/Enqueue").Value);
                 });
        }
    }
}