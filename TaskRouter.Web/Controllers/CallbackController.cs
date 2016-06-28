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
        private readonly Twilio.TwilioRestClient _client;

        public CallbackController()
        {
            _service = new MissedCallsService(new TaskRouterDbContext());
            _client = new Twilio.TwilioRestClient(Config.AccountSID, Config.AuthToken);
        }

        public CallbackController(IMissedCallsService service, Twilio.TwilioRestClient client)
        {
            _service = service;
            _client = client;
        }

        [HttpPost]
        public ActionResult Assignment()
        {
            var response = new
            {
                instruction = "dequeue",
                post_work_activity_sid = Singleton.Instance.PostWorkActivitySid
            };

            return new JsonResult() { Data = response };
        }

        [HttpPost]
        public async Task<ActionResult> Events(
            string eventType, string taskAttributes, string workerSid, string workerActivityName, string workerAttributes)
        {
            if (IsEventTimeoutOrCanceled(eventType))
            {
                await CreateMissedCallAndRedirectToVoiceMail(taskAttributes);
            }

            if (HasWorkerChangedToOffline(eventType, workerActivityName))
            {
                SendMessageToWorker(workerSid, workerAttributes);
            }

            return new EmptyResult();
        }


        private bool IsEventTimeoutOrCanceled(string eventType)
        {
            var desiredEvents = new string[] { "workflow.timeout", "task.canceled" };
            return desiredEvents.Any(e => e == eventType);
        }

        private bool HasWorkerChangedToOffline(string eventType, string workerActivityName)
        {
            return eventType == "worker.activity.update" && workerActivityName == "Offline";
        }

        private async Task CreateMissedCallAndRedirectToVoiceMail(string taskAttributes)
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

        private void SendMessageToWorker(string workerSid, string workerAttributes)
        {
            const string message = "You went offline. To make yourself available reply with \"on\"";

            dynamic attributes = JsonConvert.DeserializeObject(workerAttributes);
            string workerPhoneNumber = attributes.contact_uri;

            _client.SendMessage(Config.TwilioNumber, workerPhoneNumber, message);
            Singleton.Instance.Workers[workerPhoneNumber] = workerSid;
        }

        private void VoiceMail(string callSid)
        {
            var msg = "Sorry, All agents are busy. Please leave a message. We will call you as soon as possible";
            var routeUrl = "http://twimlets.com/voicemail?Email=" + Config.VoiceMail + "&Message=" + Url.Encode(msg);
            var client = new Twilio.TwilioRestClient(Config.AccountSID, Config.AuthToken);
            client.RedirectCall(callSid, new Twilio.CallOptions { Url = routeUrl });
        }
    }
}