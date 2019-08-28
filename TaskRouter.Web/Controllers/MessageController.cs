using System;
using System.Web.Mvc;
using TaskRouter.Web.Infrastructure;
using Twilio;
using Twilio.AspNet.Mvc;
using Twilio.Rest.Taskrouter.V1.Workspace;
using Twilio.TwiML;

namespace TaskRouter.Web.Controllers
{
    public class MessageController : TwilioController
    {
        private const string On = "on";
        private const string Off = "off";

        public MessageController()
        {
            if (Config.ENV != "test")
            {
                TwilioClient.Init(Config.AccountSID, Config.AuthToken);
            }
        }

        public virtual WorkerResource FetchWorker(string workspaceSid, string workerSid)
        {
            return WorkerResource.Fetch(workspaceSid, workerSid);
        }

        public virtual WorkerResource UpdateWorker(string pathWorkspaceSid, string pathSid, string activitySid = null,
            string attributes = null, string friendlyName = null)
        {
            return WorkerResource.Update(pathWorkspaceSid, pathSid, activitySid, attributes, friendlyName);
        }

        [HttpPost]
        public ActionResult Incoming(string from, string body)
        {
            var workspaceSid = Singleton.Instance.WorkspaceSid;
            if (!Singleton.Instance.Workers.ContainsKey(from))
            {
                return TwiML(new MessagingResponse().Message("Your number is not registered as an agent"));
            }
            var workerSid = Singleton.Instance.Workers[from];
            var idleActivitySid = Singleton.Instance.IdleActivitySid;
            var offlineActivitySid = Singleton.Instance.OfflineActivitySid;
            var message = "Unrecognized command, reply with \"on\" to activate your worker or \"off\" otherwise";

            var worker = FetchWorker(workspaceSid, workerSid);

            if (body.Equals(On, StringComparison.InvariantCultureIgnoreCase))
            {
                UpdateWorker(workspaceSid, workerSid, idleActivitySid, worker.Attributes, worker.FriendlyName);
                message = "Your worker is online";
            }

            if (body.Equals(Off, StringComparison.InvariantCultureIgnoreCase))
            {
                UpdateWorker(workspaceSid, workerSid, offlineActivitySid, worker.Attributes, worker.FriendlyName);
                message = "Your worker is offline";
            }

            return TwiML(new MessagingResponse().Message(message));
        }
    }
}