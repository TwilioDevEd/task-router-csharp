using System;
using System.Web.Mvc;
using TaskRouter.Web.Infrastructure;
using TaskRouter.Web.Models;
using TaskRouter.Web.Services;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;
using Twilio.TwiML.Voice;

namespace TaskRouter.Web.Controllers
{
    public class CallController : TwilioController
    {
        private readonly IMissedCallsService _service;

        public CallController()
        {
            _service = new MissedCallsService(new TaskRouterDbContext());
        }

        public CallController(IMissedCallsService service)
        {
            _service = service;
        }

        [HttpPost]
        public ActionResult Incoming()
        {
            var response = new VoiceResponse();
            var gather = new Gather(numDigits: 1, action: new Uri("/call/enqueue", UriKind.Relative), method: "POST");
            gather.Say("For Programmable SMS, press one. For Voice, press any other key.");
            response.Append(gather);
            return TwiML(response);
        }

        [HttpPost]
        public ActionResult Enqueue(string digits)
        {
            var selectedProduct = digits == "1" ? "ProgrammableSMS" : "ProgrammableVoice";
            var response = new VoiceResponse();
            var enqueue = new Enqueue(workflowSid: Singleton.Instance.WorkflowSid);
            enqueue.Task($"{{\"selected_product\":\"{selectedProduct}\"}}");
            response.Append(enqueue);

            return TwiML(response);
        }


    }
}