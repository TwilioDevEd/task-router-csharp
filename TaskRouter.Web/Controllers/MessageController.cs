using System;
using System.Web.Mvc;
using TaskRouter.Web.Infrastructure;
using Twilio.TaskRouter;
using Twilio.TwiML;
using Twilio.TwiML.Mvc;

namespace TaskRouter.Web.Controllers
{
    public class MessageController : TwilioController
    {

        private readonly TaskRouterClient _client;
        private const string On = "on";
        private const string Off = "off";

        public MessageController()
        {
            _client = new TaskRouterClient(Config.AccountSID, Config.AuthToken);
        }

        public MessageController(TaskRouterClient client)
        {
            _client = client;
        }

        [HttpPost]
        public ActionResult Incoming(string from, string body)
        {
            var workspaceSid = Singleton.Instance.WorkspaceSid;
            var workerSid = Singleton.Instance.Workers[from];
            var idleActivitySid = Singleton.Instance.IdleActivitySid;
            var offlineActivitySid = Singleton.Instance.OfflineActivitySid;
            var message = "Unrecognized command, reply with \"on\" to activate your worker or \"off\" otherwise";

            var worker = _client.GetWorker(workspaceSid, workerSid);

            if (body.Equals(On, StringComparison.InvariantCultureIgnoreCase))
            {
                _client.UpdateWorker(workspaceSid, workerSid, idleActivitySid, worker.Attributes, worker.FriendlyName);
                message = "Your worker is online";
            }

            if (body.Equals(Off, StringComparison.InvariantCultureIgnoreCase))
            {
                _client.UpdateWorker(workspaceSid, workerSid, offlineActivitySid, worker.Attributes, worker.FriendlyName);
                message = "Your worker is offline";
            }

            return TwiML(new TwilioResponse().Message(message));
        }
    }
}