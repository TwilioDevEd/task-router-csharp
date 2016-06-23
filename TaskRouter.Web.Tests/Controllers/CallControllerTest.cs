﻿using NUnit.Framework;
using System.Xml.XPath;
using TaskRouter.Web.Controllers;
using TaskRouter.Web.Tests.Extensions;
using TestStack.FluentMVCTesting;

namespace TaskRouter.Web.Tests.Controllers
{
    public class CallControllerTest
    {
        [Test]
        public void Incoming()
        {
            var controller = new CallController();
            controller.WithCallTo(c => c.Incoming())
                .ShouldReturnTwiMLResult(data =>
                 {
                     StringAssert.Contains(
                         "For Programmable SMS", data.XPathSelectElement("Response/Gather/Say").Value);
                 });
        }

        [TestCase("1", "ProgrammableSMS")]
        [TestCase("2", "ProgrammableVoice")]
        public void Enqueue(string digits, string selectedProduct)
        {
            var controller = new CallController();
            controller.WithCallTo(c => c.Enqueue(digits))
                .ShouldReturnTwiMLResult(data =>
                 {
                     StringAssert.Contains(
                         selectedProduct, data.XPathSelectElement("Response/Enqueue/Task").Value);
                 });
        }

        [Test]
        public void Assignment()
        {
            var controller = new CallController();
            controller.WithCallTo(c => c.Assignment())
                .ShouldReturnJson(data =>
                 {
                     Assert.That(data.instruction, Is.EqualTo("dequeue"));
                 });
        }
    }
}
