using System.Web.Mvc;
using TaskRouter.Web.Infrastructure;
using TaskRouter.Web.Models;
using TaskRouter.Web.Services;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;

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
            var gather = new Gather(numDigits: 1, action: "/call/enqueue", method: "POST");
            gather.Say("For Programmable SMS, press one. For Voice, press any other key.");
            response.Gather(gather);

            return TwiML(response);
        }

        [HttpPost]
        public ActionResult Enqueue(string digits)
        {
            var selectedProduct = digits == "1" ? "ProgrammableSMS" : "ProgrammableVoice";
            var response = new VoiceResponse();

            response.Enqueue(
                selectedProduct,
                workflowSid: Singleton.Instance.WorkflowSid);

            return TwiML(response);
        }


    }
}