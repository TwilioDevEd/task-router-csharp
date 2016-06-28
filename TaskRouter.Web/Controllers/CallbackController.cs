using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using TaskRouter.Web.Infrastructure;
using TaskRouter.Web.Models;
using TaskRouter.Web.Services;

namespace TaskRouter.Web.Controllers
{
    public class CallbackController : Controller
    {
        private readonly IMissedCallsService _service;

        public CallbackController()
        {
            _service = new MissedCallsService(new TaskRouterDbContext());
        }

        public CallbackController(IMissedCallsService service)
        {
            _service = service;
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
        public async Task<ActionResult> Events(string eventType, string taskAttributes)
        {
            var desiredEvents = new string[] { "workflow.timeout", "task.canceled" };

            if (desiredEvents.Any(e => e == eventType))
            {
                dynamic attributes = JsonConvert.DeserializeObject(taskAttributes);
                var missedCall = new MissedCall
                {
                    PhoneNumber = attributes.from,
                    Product = attributes.selected_product,
                    CreatedAt = DateTime.Now
                };

                await _service.CreateAsync(missedCall);
                string voiceSid = attributes.call_sid;
                VoiceMail(voiceSid);
            }

            return new EmptyResult();
        }

        private void VoiceMail(string callSid)
        {
            var msg = "Sorry, All agents are busy. Please leave a message. We will call you as soon as possible";
            var routeUrl = "http://twimlets.com/voicemail?Email=" + "agustin.camino@gmail.com" + "&Message=" + msg;
            var client = new Twilio.TwilioRestClient(Config.AccountSID, Config.AuthToken);
            client.RedirectCall(callSid, new Twilio.CallOptions { Url = routeUrl });
        }
    }
}