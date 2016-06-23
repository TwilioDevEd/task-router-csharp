using System.Web.Mvc;
using TaskRouter.Web.Infraestructure;
using Twilio.TwiML;
using Twilio.TwiML.Mvc;

namespace TaskRouter.Web.Controllers
{
    public class CallController : TwilioController
    {
        [HttpPost]
        public ActionResult Incoming()
        {
            var response = new TwilioResponse();
            response
                .BeginGather(new { numDigits = 1, action = "/call/enqueue", method = "POST" })
                .Say("For Programmable SMS, press one. For Voice, press any other key.")
                .EndGather();

            return TwiML(response);
        }

        [HttpPost]
        public ActionResult Enqueue(string digits)
        {
            var selectedProduct = digits == "1" ? "ProgrammableSMS" : "ProgrammableVoice";

            var response = new TwilioResponse();
            response.EnqueueTask(
                new { workflowSid = Singleton.Instance.WorkflowSid },
                new Task("{\"selected_product\":\"" + selectedProduct + "\"}"));

            return TwiML(response);
        }

        [HttpPost]
        public ActionResult Assignment()
        {
            var response = new {
                instruction = "dequeue",
                post_work_activity_sid = Singleton.Instance.PostWorkActivitySid
            };

            return new JsonResult() { Data = response };
        }

        [HttpPost]
        public ActionResult Events(string eventType, string taskAttributes)
        {
            var desiredEvents = new string[] { "workflow.timeout", "task.canceled" };

            if (eventType == "workflow.timeout" || eventType == "task.canceled")
            {
                RouteCall("");
            }

            return new EmptyResult();
        }

        private void RouteCall(string callSid)
        {
            var client = new Twilio.TwilioRestClient(Config.AccountSID, Config.AuthToken);
            var call = client.GetCall("callSid");
            client.RedirectCall("callSid", new Twilio.CallOptions { Url = "https://sb.ngrok.io/farewell" });
        }
    }
}